using CobranzaCloud.Api.Extensions;
using CobranzaCloud.Api.Middleware;
using CobranzaCloud.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Serilog;

// =============================================================================
// CobranzaCloud API - Entry Point
// =============================================================================

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

Log.Information("Starting CobranzaCloud API...");

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Serilog
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console());

    // OpenAPI / Swagger
    builder.Services.AddOpenApi();

    // Application Services (Database, MediatR, Validation, Health Checks)
    builder.Services.AddApplicationServices(builder.Configuration);

    // CORS
    builder.Services.AddApiCors(builder.Configuration);

    var app = builder.Build();

    // Error Handling Middleware (first in pipeline)
    app.UseErrorHandling();

    // Request Logging
    app.UseSerilogRequestLogging();

    // CORS
    app.UseCors("AllowFrontend");

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();

        // Auto-migrate in development (FRICTIONLESS)
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
        Log.Information("Database migrations applied");
    }

    // Health check endpoint
    app.MapHealthChecks("/health");

    // Root endpoint (API info)
    app.MapGet("/", () => new
    {
        name = "CobranzaCloud API",
        version = "0.1.0",
        status = "running",
        environment = app.Environment.EnvironmentName,
        docs = "/openapi/v1.json"
    })
    .WithName("GetApiRoot")
    .WithTags("System")
    .ExcludeFromDescription();

    // Map all API endpoints
    app.MapApiEndpoints();

    Log.Information("CobranzaCloud API started successfully on {Environment}", app.Environment.EnvironmentName);
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
