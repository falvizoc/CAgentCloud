using System.Security.Claims;
using CobranzaCloud.Api.Extensions;
using CobranzaCloud.Application.Cartera;
using CobranzaCloud.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CobranzaCloud.Api.Endpoints;

/// <summary>
/// Client list endpoints for dashboard
/// </summary>
public static class ClientesEndpoints
{
    public static void MapClientesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/clientes")
            .WithTags("Clientes")
            .RequireAuthorization("CarteraRead")
            .WithOpenApi();

        group.MapGet("/", GetClientes)
            .WithName("GetClientes")
            .WithDescription("Get paginated list of clients with optional filters")
            .Produces<ClientesListResponse>(200)
            .Produces<ProblemDetails>(401);
    }

    private static async Task<IResult> GetClientes(
        ClaimsPrincipal principal,
        AppDbContext db,
        ILogger<Program> logger,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] bool? conSaldo = null,
        [FromQuery] string? orderBy = "saldoVencido",
        [FromQuery] string? orderDir = "desc",
        CancellationToken ct = default)
    {
        var orgId = principal.GetOrganizationId();

        logger.LogDebug(
            "Getting clientes for org {OrgId}: page={Page}, pageSize={PageSize}, search={Search}, conSaldo={ConSaldo}",
            orgId, page, pageSize, search, conSaldo);

        // Validate pagination
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = db.Clientes
            .Where(c => c.OrganizationId == orgId);

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.ToLower();
            query = query.Where(c =>
                c.Nombre.ToLower().Contains(searchLower) ||
                c.Clave.ToLower().Contains(searchLower));
        }

        // Apply conSaldo filter
        if (conSaldo == true)
        {
            query = query.Where(c => c.SaldoTotal > 0);
        }

        // Get total count before pagination
        var total = await query.CountAsync(ct);

        // Apply ordering
        query = (orderBy?.ToLower(), orderDir?.ToLower()) switch
        {
            ("nombre", "asc") => query.OrderBy(c => c.Nombre),
            ("nombre", _) => query.OrderByDescending(c => c.Nombre),
            ("clave", "asc") => query.OrderBy(c => c.Clave),
            ("clave", _) => query.OrderByDescending(c => c.Clave),
            ("saldototal", "asc") => query.OrderBy(c => c.SaldoTotal),
            ("saldototal", _) => query.OrderByDescending(c => c.SaldoTotal),
            ("diasmaxvencido", "asc") => query.OrderBy(c => c.DiasMaxVencido),
            ("diasmaxvencido", _) => query.OrderByDescending(c => c.DiasMaxVencido),
            // Default: order by saldoVencido desc, then by diasMaxVencido desc (most urgent first)
            _ => query.OrderByDescending(c => c.SaldoVencido).ThenByDescending(c => c.DiasMaxVencido)
        };

        // Apply pagination
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new ClienteListItem(
                c.Id,
                c.Clave,
                c.Nombre,
                c.SaldoTotal,
                c.SaldoVencido,
                c.DiasMaxVencido,
                c.FacturasActivas
            ))
            .ToListAsync(ct);

        var response = new ClientesListResponse(
            items,
            new PaginationMeta(
                Page: page,
                PageSize: pageSize,
                Total: total,
                TotalPages: (int)Math.Ceiling(total / (double)pageSize)
            )
        );

        return Results.Ok(response);
    }
}
