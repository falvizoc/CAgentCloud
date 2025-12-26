using System.Security.Claims;
using CobranzaCloud.Api.Extensions;
using CobranzaCloud.Application.Cartera;
using CobranzaCloud.Core.Entities;
using CobranzaCloud.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CobranzaCloud.Api.Endpoints;

/// <summary>
/// Portfolio/cartera summary endpoints for dashboard
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
            .WithDescription("Get portfolio summary with totals and KPIs")
            .Produces<CarteraResumenResponse>(200)
            .Produces<ProblemDetails>(401);

        group.MapGet("/antiguedad", GetAntiguedad)
            .WithName("GetCarteraAntiguedad")
            .WithDescription("Get portfolio aging report by range")
            .Produces<CarteraAntiguedadResponse>(200)
            .Produces<ProblemDetails>(401);
    }

    private static async Task<IResult> GetResumen(
        ClaimsPrincipal principal,
        AppDbContext db,
        ILogger<Program> logger,
        CancellationToken ct)
    {
        var orgId = principal.GetOrganizationId();

        logger.LogDebug("Getting cartera resumen for org {OrgId}", orgId);

        var clientes = await db.Clientes
            .Where(c => c.OrganizationId == orgId)
            .ToListAsync(ct);

        var totalCartera = clientes.Sum(c => c.SaldoTotal);
        var carteraVencida = clientes.Sum(c => c.SaldoVencido);
        var carteraVigente = totalCartera - carteraVencida;

        var ultimaSync = await db.Connectors
            .Where(c => c.OrganizationId == orgId)
            .Select(c => c.LastSyncAt)
            .MaxAsync(ct);

        var response = new CarteraResumenResponse(
            TotalCartera: totalCartera,
            CarteraVigente: carteraVigente,
            CarteraVencida: carteraVencida,
            PorcentajeVencido: totalCartera > 0 ? (carteraVencida / totalCartera) * 100 : 0,
            ClientesConSaldo: clientes.Count(c => c.SaldoTotal > 0),
            FacturasActivas: clientes.Sum(c => c.FacturasActivas),
            UltimaSincronizacion: ultimaSync
        );

        return Results.Ok(response);
    }

    private static async Task<IResult> GetAntiguedad(
        ClaimsPrincipal principal,
        AppDbContext db,
        ILogger<Program> logger,
        CancellationToken ct)
    {
        var orgId = principal.GetOrganizationId();

        logger.LogDebug("Getting cartera antiguedad for org {OrgId}", orgId);

        var facturas = await db.Facturas
            .Where(f => f.Cliente.OrganizationId == orgId)
            .Where(f => f.Status != FacturaStatus.Pagada && f.Status != FacturaStatus.Cancelada)
            .ToListAsync(ct);

        var total = facturas.Sum(f => f.Saldo);

        var rangos = facturas
            .GroupBy(f => f.RangoAntiguedad)
            .Select(g => new RangoAntiguedadItem(
                Rango: g.Key.ToString(),
                Label: GetRangoLabel(g.Key),
                Monto: g.Sum(f => f.Saldo),
                Facturas: g.Count(),
                Porcentaje: total > 0 ? Math.Round((g.Sum(f => f.Saldo) / total) * 100, 2) : 0
            ))
            .OrderBy(r => GetRangoOrder(r.Rango))
            .ToList();

        // Ensure all ranges are present even if empty
        var allRanges = EnsureAllRanges(rangos, total);

        var response = new CarteraAntiguedadResponse(allRanges, total);

        return Results.Ok(response);
    }

    private static string GetRangoLabel(RangoAntiguedad rango) => rango switch
    {
        RangoAntiguedad.Vigente => "Vigente",
        RangoAntiguedad.Dias1a30 => "1-30 días",
        RangoAntiguedad.Dias31a60 => "31-60 días",
        RangoAntiguedad.Dias61a90 => "61-90 días",
        RangoAntiguedad.MasDe90 => "Más de 90 días",
        _ => "Desconocido"
    };

    private static int GetRangoOrder(string rango) => rango switch
    {
        "Vigente" => 0,
        "Dias1a30" => 1,
        "Dias31a60" => 2,
        "Dias61a90" => 3,
        "MasDe90" => 4,
        _ => 99
    };

    private static List<RangoAntiguedadItem> EnsureAllRanges(List<RangoAntiguedadItem> rangos, decimal total)
    {
        var rangoNames = new[]
        {
            (Rango: "Vigente", Label: "Vigente"),
            (Rango: "Dias1a30", Label: "1-30 días"),
            (Rango: "Dias31a60", Label: "31-60 días"),
            (Rango: "Dias61a90", Label: "61-90 días"),
            (Rango: "MasDe90", Label: "Más de 90 días")
        };

        var result = new List<RangoAntiguedadItem>();
        foreach (var (rango, label) in rangoNames)
        {
            var existing = rangos.FirstOrDefault(r => r.Rango == rango);
            if (existing != null)
            {
                result.Add(existing);
            }
            else
            {
                result.Add(new RangoAntiguedadItem(rango, label, 0, 0, 0));
            }
        }

        return result;
    }
}
