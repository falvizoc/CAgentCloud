using System.Security.Claims;
using CobranzaCloud.Api.Extensions;
using CobranzaCloud.Application.Cartera;
using CobranzaCloud.Application.ExternalServices;
using CobranzaCloud.Core.Entities;
using CobranzaCloud.Infrastructure.Data;
using CobranzaCloud.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CobranzaCloud.Api.Endpoints;

/// <summary>
/// Portfolio/cartera summary endpoints for dashboard
/// Fetches data from Cobranza Agent (ASPEL connector) with Redis caching
/// </summary>
public static class CarteraEndpoints
{
    public static void MapCarteraEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/cartera")
            .WithTags("Cartera")
            .RequireAuthorization("CarteraRead")
            .WithOpenApi();

        group.MapGet("/resumen", GetResumen)
            .WithName("GetCarteraResumen")
            .WithDescription("Get portfolio summary with totals and KPIs from ASPEL connector")
            .Produces<CarteraResumenResponse>(200)
            .Produces<ProblemDetails>(401)
            .Produces<ProblemDetails>(503);

        group.MapGet("/antiguedad", GetAntiguedad)
            .WithName("GetCarteraAntiguedad")
            .WithDescription("Get portfolio aging report by range from ASPEL connector")
            .Produces<CarteraAntiguedadResponse>(200)
            .Produces<ProblemDetails>(401)
            .Produces<ProblemDetails>(503);

        group.MapPost("/refresh", RefreshCache)
            .WithName("RefreshCarteraCache")
            .WithDescription("Force refresh cartera data from connector")
            .Produces(204)
            .Produces<ProblemDetails>(401);
    }

    private static async Task<IResult> GetResumen(
        ClaimsPrincipal principal,
        ICobranzaAgentClient agentClient,
        ICacheService cache,
        IOptions<CobranzaAgentOptions> options,
        ILogger<Program> logger,
        CancellationToken ct)
    {
        var orgId = principal.GetOrganizationId();
        var empresaId = options.Value.DefaultEmpresaId;
        var moneda = options.Value.DefaultMoneda; // MUST: MXN by default (DEC-009)

        logger.LogDebug("Getting cartera resumen for org {OrgId}, empresa {EmpresaId}, moneda {Moneda}",
            orgId, empresaId, moneda);

        // Try cache first
        var cacheKey = CacheKeys.CarteraResumen(orgId, empresaId, moneda == 1 ? "MXN" : "USD");

        var response = await cache.GetOrSetAsync(
            cacheKey,
            async () =>
            {
                logger.LogInformation("Cache miss - fetching from connector: {CacheKey}", cacheKey);

                var agentResponse = await agentClient.GetCarteraResumenAsync(empresaId, moneda, ct);

                if (agentResponse?.Success != true || agentResponse.Data == null)
                {
                    logger.LogWarning("Failed to fetch cartera resumen from connector");
                    return null;
                }

                var data = agentResponse.Data;
                return new CarteraResumenResponse(
                    TotalCartera: data.TotalCartera,
                    CarteraVigente: data.CarteraPorVencer,
                    CarteraVencida: data.CarteraVencida,
                    PorcentajeVencido: data.TotalCartera > 0
                        ? Math.Round((data.CarteraVencida / data.TotalCartera) * 100, 2)
                        : 0,
                    ClientesConSaldo: data.ClientesConSaldo,
                    FacturasActivas: data.TotalFacturas,
                    UltimaSincronizacion: DateTime.UtcNow
                );
            },
            CacheKeys.DefaultExpiration,
            ct
        );

        if (response == null)
        {
            return Results.Problem(
                title: "Connector unavailable",
                detail: "Unable to fetch data from ASPEL connector. Please try again later.",
                statusCode: 503
            );
        }

        return Results.Ok(response);
    }

    private static async Task<IResult> GetAntiguedad(
        ClaimsPrincipal principal,
        ICobranzaAgentClient agentClient,
        ICacheService cache,
        IOptions<CobranzaAgentOptions> options,
        ILogger<Program> logger,
        CancellationToken ct)
    {
        var orgId = principal.GetOrganizationId();
        var empresaId = options.Value.DefaultEmpresaId;
        var moneda = options.Value.DefaultMoneda; // MUST: MXN by default (DEC-009)

        logger.LogDebug("Getting cartera antiguedad for org {OrgId}, empresa {EmpresaId}", orgId, empresaId);

        // Try cache first
        var cacheKey = CacheKeys.CarteraAntiguedad(orgId, empresaId, moneda == 1 ? "MXN" : "USD");

        var response = await cache.GetOrSetAsync(
            cacheKey,
            async () =>
            {
                logger.LogInformation("Cache miss - fetching antiguedad from connector: {CacheKey}", cacheKey);

                var agentResponse = await agentClient.GetCarteraAntiguedadAsync(empresaId, moneda, ct);

                if (agentResponse?.Success != true || agentResponse.Data == null)
                {
                    logger.LogWarning("Failed to fetch cartera antiguedad from connector");
                    return null;
                }

                var data = agentResponse.Data;

                // Map connector ranges to our format
                var rangos = new List<RangoAntiguedadItem>
                {
                    new("Vigente", "Vigente", data.Corriente, 0,
                        data.Total > 0 ? Math.Round((data.Corriente / data.Total) * 100, 2) : 0),
                    new("Dias1a30", "1-30 días", data.Rango1a30, 0,
                        data.Total > 0 ? Math.Round((data.Rango1a30 / data.Total) * 100, 2) : 0),
                    new("Dias31a60", "31-60 días", data.Rango31a60, 0,
                        data.Total > 0 ? Math.Round((data.Rango31a60 / data.Total) * 100, 2) : 0),
                    new("Dias61a90", "61-90 días", data.Rango61a90, 0,
                        data.Total > 0 ? Math.Round((data.Rango61a90 / data.Total) * 100, 2) : 0),
                    new("MasDe90", "Más de 90 días", data.RangoMas90, 0,
                        data.Total > 0 ? Math.Round((data.RangoMas90 / data.Total) * 100, 2) : 0)
                };

                return new CarteraAntiguedadResponse(rangos, data.Total);
            },
            CacheKeys.DefaultExpiration,
            ct
        );

        if (response == null)
        {
            return Results.Problem(
                title: "Connector unavailable",
                detail: "Unable to fetch aging data from ASPEL connector. Please try again later.",
                statusCode: 503
            );
        }

        return Results.Ok(response);
    }

    private static async Task<IResult> RefreshCache(
        ClaimsPrincipal principal,
        ICacheService cache,
        IOptions<CobranzaAgentOptions> options,
        ILogger<Program> logger,
        CancellationToken ct)
    {
        var orgId = principal.GetOrganizationId();
        var empresaId = options.Value.DefaultEmpresaId;

        logger.LogInformation("Refreshing cartera cache for org {OrgId}, empresa {EmpresaId}", orgId, empresaId);

        // Invalidate all cartera cache for this org/empresa
        await cache.RemoveByPatternAsync(CacheKeys.EmpresaPattern(orgId, empresaId), ct);

        return Results.NoContent();
    }
}
