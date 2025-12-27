using System.Text;
using CobranzaCloud.Application.Auth;
using CobranzaCloud.Application.Connectors;
using CobranzaCloud.Application.ExternalServices;
using CobranzaCloud.Infrastructure.Data;
using CobranzaCloud.Infrastructure.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;

namespace CobranzaCloud.Api.Extensions;

/// <summary>
/// Extension methods for IServiceCollection to configure application services
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Database
        services.AddDatabase(configuration);

        // Authentication
        services.AddJwtAuthentication(configuration);

        // MediatR
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(CobranzaCloud.Application.AssemblyReference).Assembly);
        });

        // FluentValidation
        services.AddValidatorsFromAssembly(typeof(CobranzaCloud.Application.AssemblyReference).Assembly);

        // Health Checks
        services.AddHealthChecks()
            .AddNpgSql(configuration.GetConnectionString("DefaultConnection") ?? "");

        // Redis Cache
        services.AddRedisCache(configuration);

        // Cobranza Agent Client (ASPEL Connector)
        services.AddCobranzaAgentClient(configuration);

        return services;
    }

    public static IServiceCollection AddRedisCache(this IServiceCollection services, IConfiguration configuration)
    {
        var redisConnection = configuration.GetConnectionString("Redis") ?? "localhost:6379";

        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var options = ConfigurationOptions.Parse(redisConnection);
            options.AbortOnConnectFail = false;
            return ConnectionMultiplexer.Connect(options);
        });

        services.AddScoped<ICacheService, RedisCacheService>();

        return services;
    }

    public static IServiceCollection AddCobranzaAgentClient(this IServiceCollection services, IConfiguration configuration)
    {
        // Bind configuration
        services.Configure<CobranzaAgentOptions>(configuration.GetSection(CobranzaAgentOptions.SectionName));

        // Register HttpClient with configuration
        services.AddHttpClient<ICobranzaAgentClient, CobranzaAgentClient>()
            .ConfigureHttpClient((sp, client) =>
            {
                var options = configuration.GetSection(CobranzaAgentOptions.SectionName).Get<CobranzaAgentOptions>()
                    ?? new CobranzaAgentOptions();
                client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
            });

        return services;
    }

    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        // Bind JWT settings
        var jwtSettings = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
            ?? throw new InvalidOperationException("JWT settings not configured");

        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IConnectorService, ConnectorService>();

        // Configure JWT authentication
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
                ClockSkew = TimeSpan.Zero // No tolerance for expired tokens
            };

            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    if (context.Exception is SecurityTokenExpiredException)
                    {
                        context.Response.Headers.Append("Token-Expired", "true");
                    }
                    return Task.CompletedTask;
                }
            };
        });

        // Authorization policies
        services.AddAuthorizationBuilder()
            .AddPolicy("CarteraRead", policy =>
                policy.RequireAssertion(ctx =>
                    ctx.User.HasClaim("permissions", "cartera:read") ||
                    ctx.User.HasClaim("permissions", "cartera:*") ||
                    ctx.User.IsInRole("admin") ||
                    ctx.User.IsInRole("owner")))
            .AddPolicy("CarteraWrite", policy =>
                policy.RequireAssertion(ctx =>
                    ctx.User.HasClaim("permissions", "cartera:write") ||
                    ctx.User.HasClaim("permissions", "cartera:*") ||
                    ctx.User.IsInRole("admin") ||
                    ctx.User.IsInRole("owner")))
            .AddPolicy("UsersManage", policy =>
                policy.RequireAssertion(ctx =>
                    ctx.User.HasClaim("permissions", "users:*") ||
                    ctx.User.IsInRole("admin") ||
                    ctx.User.IsInRole("owner")));

        return services;
    }

    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
                npgsqlOptions.EnableRetryOnFailure(3);
            }));

        return services;
    }

    public static IServiceCollection AddApiCors(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend", policy =>
            {
                var origins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                    ?? new[] { "http://localhost:3000", "https://localhost:3000" };

                policy.WithOrigins(origins)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            });
        });

        return services;
    }
}
