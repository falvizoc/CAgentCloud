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

        group.MapGet("/{id:guid}", GetClienteById)
            .WithName("GetClienteById")
            .WithDescription("Get client detail by ID including contacts and invoices")
            .Produces<ClienteDetailResponse>(200)
            .Produces<ProblemDetails>(401)
            .Produces<ProblemDetails>(404);
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

    private static async Task<IResult> GetClienteById(
        Guid id,
        ClaimsPrincipal principal,
        AppDbContext db,
        ILogger<Program> logger,
        CancellationToken ct)
    {
        var orgId = principal.GetOrganizationId();

        logger.LogDebug("Getting cliente {ClienteId} for org {OrgId}", id, orgId);

        var cliente = await db.Clientes
            .Include(c => c.Contactos)
            .Include(c => c.Facturas.Where(f => f.Status != Core.Entities.FacturaStatus.Pagada
                                              && f.Status != Core.Entities.FacturaStatus.Cancelada))
            .FirstOrDefaultAsync(c => c.Id == id && c.OrganizationId == orgId, ct);

        if (cliente == null)
        {
            return Results.Problem(
                title: "Cliente no encontrado",
                detail: "El cliente solicitado no existe o no pertenece a su organizacion",
                statusCode: 404
            );
        }

        var direccion = (cliente.Calle != null || cliente.Colonia != null ||
                        cliente.Ciudad != null || cliente.Estado != null || cliente.CodigoPostal != null)
            ? new DireccionDto(
                cliente.Calle,
                cliente.Colonia,
                cliente.Ciudad,
                cliente.Estado,
                cliente.CodigoPostal
            )
            : null;

        var response = new ClienteDetailResponse(
            Id: cliente.Id,
            Clave: cliente.Clave,
            Nombre: cliente.Nombre,
            Rfc: cliente.Rfc,
            Email: cliente.Email,
            Telefono: cliente.Telefono,
            Direccion: direccion,
            SaldoTotal: cliente.SaldoTotal,
            SaldoVencido: cliente.SaldoVencido,
            DiasMaxVencido: cliente.DiasMaxVencido,
            FacturasActivas: cliente.FacturasActivas,
            UltimoPago: cliente.UltimoPago,
            LastSyncAt: cliente.LastSyncAt,
            Contactos: cliente.Contactos
                .OrderByDescending(c => c.Principal)
                .ThenBy(c => c.Nombre)
                .Select(c => new ContactoDto(
                    c.Id,
                    c.Nombre,
                    c.Email,
                    c.Telefono,
                    c.Principal
                ))
                .ToList(),
            Facturas: cliente.Facturas
                .OrderByDescending(f => f.DiasVencido)
                .ThenByDescending(f => f.Saldo)
                .Select(f => new FacturaDto(
                    f.Id,
                    f.Folio,
                    f.Fecha,
                    f.Vencimiento,
                    f.Total,
                    f.Saldo,
                    f.DiasVencido,
                    f.Status.ToString()
                ))
                .ToList()
        );

        return Results.Ok(response);
    }
}
