using System.Security.Claims;
using CobranzaCloud.Api.Extensions;
using CobranzaCloud.Application.Cartera;
using CobranzaCloud.Application.ExternalServices;
using CobranzaCloud.Infrastructure.Data;
using CobranzaCloud.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CobranzaCloud.Api.Endpoints;

/// <summary>
/// Client list endpoints for dashboard
/// Fetches data from Cobranza Agent (ASPEL connector) with Redis caching
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
            .WithDescription("Get paginated list of clients from ASPEL connector")
            .Produces<ClientesListResponse>(200)
            .Produces<ProblemDetails>(401)
            .Produces<ProblemDetails>(503);

        group.MapGet("/{clave}", GetClienteByClave)
            .WithName("GetClienteByClave")
            .WithDescription("Get client detail by clave from ASPEL connector")
            .Produces<ClienteDetailResponse>(200)
            .Produces<ProblemDetails>(401)
            .Produces<ProblemDetails>(404)
            .Produces<ProblemDetails>(503);
    }

    private static async Task<IResult> GetClientes(
        ClaimsPrincipal principal,
        ICobranzaAgentClient agentClient,
        ICacheService cache,
        IOptions<CobranzaAgentOptions> options,
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
        var empresaId = options.Value.DefaultEmpresaId;

        logger.LogDebug(
            "Getting clientes for org {OrgId}: page={Page}, pageSize={PageSize}, search={Search}",
            orgId, page, pageSize, search);

        // Validate pagination
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        // Cache key for the full client list (we cache all clients and filter in memory)
        var cacheKey = CacheKeys.Clientes(orgId, empresaId);

        var allClientes = await cache.GetOrSetAsync(
            cacheKey,
            async () =>
            {
                logger.LogInformation("Cache miss - fetching clientes from connector: {CacheKey}", cacheKey);

                // Fetch all clients from connector (using limit=500 for now)
                var agentResponse = await agentClient.GetClientesAsync(empresaId, 500, 0, ct);

                if (agentResponse?.Success != true || agentResponse.Items == null)
                {
                    logger.LogWarning("Failed to fetch clientes from connector");
                    return null;
                }

                return agentResponse.Items.Select(c => new ClienteListItem(
                    Guid.NewGuid(), // Temporary ID for frontend compatibility
                    c.Clave,
                    c.Nombre,
                    c.SaldoTotal,
                    c.SaldoVencido,
                    0, // DiasMaxVencido not available in list
                    c.FacturasPendientes
                )).ToList();
            },
            CacheKeys.DefaultExpiration,
            ct
        );

        if (allClientes == null)
        {
            return Results.Problem(
                title: "Connector unavailable",
                detail: "Unable to fetch client data from ASPEL connector. Please try again later.",
                statusCode: 503
            );
        }

        // Apply in-memory filters
        var filtered = allClientes.AsEnumerable();

        // Search filter
        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.ToLower();
            filtered = filtered.Where(c =>
                c.Nombre.ToLower().Contains(searchLower) ||
                c.Clave.ToLower().Contains(searchLower));
        }

        // ConSaldo filter
        if (conSaldo == true)
        {
            filtered = filtered.Where(c => c.SaldoTotal > 0);
        }

        // Ordering
        filtered = (orderBy?.ToLower(), orderDir?.ToLower()) switch
        {
            ("nombre", "asc") => filtered.OrderBy(c => c.Nombre),
            ("nombre", _) => filtered.OrderByDescending(c => c.Nombre),
            ("clave", "asc") => filtered.OrderBy(c => c.Clave),
            ("clave", _) => filtered.OrderByDescending(c => c.Clave),
            ("saldototal", "asc") => filtered.OrderBy(c => c.SaldoTotal),
            ("saldototal", _) => filtered.OrderByDescending(c => c.SaldoTotal),
            _ => filtered.OrderByDescending(c => c.SaldoVencido)
        };

        var filteredList = filtered.ToList();
        var total = filteredList.Count;

        // Apply pagination
        var items = filteredList
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

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

    private static async Task<IResult> GetClienteByClave(
        string clave,
        ClaimsPrincipal principal,
        ICobranzaAgentClient agentClient,
        ICacheService cache,
        IOptions<CobranzaAgentOptions> options,
        ILogger<Program> logger,
        CancellationToken ct)
    {
        var orgId = principal.GetOrganizationId();
        var empresaId = options.Value.DefaultEmpresaId;

        logger.LogDebug("Getting cliente {Clave} for org {OrgId}", clave, orgId);

        var cacheKey = CacheKeys.ClienteDetalle(orgId, empresaId, clave);

        var clienteDetalle = await cache.GetOrSetAsync(
            cacheKey,
            async () =>
            {
                logger.LogInformation("Cache miss - fetching cliente from connector: {CacheKey}", cacheKey);

                var agentResponse = await agentClient.GetClienteDetalleAsync(empresaId, clave, ct);

                if (agentResponse?.Success != true || agentResponse.Data == null)
                {
                    logger.LogWarning("Failed to fetch cliente {Clave} from connector", clave);
                    return null;
                }

                var data = agentResponse.Data;
                return new ClienteDetailResponse(
                    Id: Guid.NewGuid(), // Temporary ID for frontend compatibility
                    Clave: data.Clave,
                    Nombre: data.Nombre,
                    Rfc: data.Rfc,
                    Email: data.Email ?? data.Emails?.FirstOrDefault(),
                    Telefono: data.Telefono ?? data.Celular,
                    Direccion: null, // Not available in connector
                    SaldoTotal: 0, // Will be populated from list
                    SaldoVencido: 0, // Will be populated from list
                    DiasMaxVencido: 0,
                    FacturasActivas: 0,
                    UltimoPago: null,
                    LastSyncAt: DateTime.UtcNow,
                    Contactos: data.Contacto != null
                        ? new List<ContactoDto>
                        {
                            new ContactoDto(
                                Guid.NewGuid(),
                                data.Contacto,
                                data.Emails?.FirstOrDefault(),
                                data.Telefono,
                                true
                            )
                        }
                        : new List<ContactoDto>(),
                    Facturas: new List<FacturaDto>() // Will need separate call to get facturas
                );
            },
            CacheKeys.DefaultExpiration,
            ct
        );

        if (clienteDetalle == null)
        {
            return Results.Problem(
                title: "Cliente no encontrado",
                detail: $"El cliente con clave '{clave}' no existe en el sistema ASPEL.",
                statusCode: 404
            );
        }

        return Results.Ok(clienteDetalle);
    }
}
