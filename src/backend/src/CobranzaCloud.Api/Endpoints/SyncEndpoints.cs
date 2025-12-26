using System.Security.Claims;
using CobranzaCloud.Application.Sync;
using CobranzaCloud.Core.Entities;
using CobranzaCloud.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CobranzaCloud.Api.Endpoints;

/// <summary>
/// Data synchronization endpoints for connectors
/// </summary>
public static class SyncEndpoints
{
    public static void MapSyncEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/sync")
            .WithTags("Synchronization")
            .RequireAuthorization()
            .WithOpenApi();

        group.MapPost("/cartera", SyncCartera)
            .WithName("SyncCartera")
            .WithDescription("Receive portfolio data from connector")
            .Produces<SyncCarteraResponse>(200)
            .Produces<ProblemDetails>(400)
            .Produces<ProblemDetails>(401);
    }

    private static async Task<IResult> SyncCartera(
        [FromBody] SyncCarteraRequest request,
        ClaimsPrincipal principal,
        AppDbContext db,
        ILogger<Program> logger,
        CancellationToken ct)
    {
        // Verify this is a connector token
        var tokenType = principal.FindFirst("type")?.Value;
        if (tokenType != "connector")
        {
            return Results.Problem(
                title: "Invalid token type",
                detail: "This endpoint requires a connector token",
                statusCode: 401
            );
        }

        var connectorIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var orgIdClaim = principal.FindFirst("org_id")?.Value;

        if (connectorIdClaim == null || !Guid.TryParse(connectorIdClaim, out var connectorId))
        {
            return Results.Problem(
                title: "Invalid token",
                detail: "Connector ID not found in token",
                statusCode: 401
            );
        }

        if (orgIdClaim == null || !Guid.TryParse(orgIdClaim, out var orgId))
        {
            return Results.Problem(
                title: "Invalid token",
                detail: "Organization ID not found in token",
                statusCode: 401
            );
        }

        var syncId = Guid.NewGuid().ToString();
        var stats = new SyncStats(0, 0, 0, 0, 0);
        var nuevos = 0;
        var modificados = 0;
        var sinCambios = 0;
        var facturasActualizadas = 0;

        logger.LogInformation(
            "Starting sync {SyncId} for org {OrgId} from connector {ConnectorId}: {ClientCount} clients",
            syncId, orgId, connectorId, request.Data.Clientes.Count);

        foreach (var clienteDto in request.Data.Clientes)
        {
            if (clienteDto.Operation == "delete")
            {
                // Soft delete or skip - for now we skip deletes
                continue;
            }

            // Find or create client
            var cliente = await db.Clientes
                .Include(c => c.Facturas)
                .FirstOrDefaultAsync(c => c.Clave == clienteDto.Clave && c.OrganizationId == orgId, ct);

            var isNew = cliente == null;

            if (isNew)
            {
                cliente = new Cliente
                {
                    Id = Guid.NewGuid(),
                    Clave = clienteDto.Clave,
                    Nombre = clienteDto.Nombre,
                    OrganizationId = orgId,
                    CreatedAt = DateTime.UtcNow
                };
                db.Clientes.Add(cliente);
                nuevos++;
            }
            else
            {
                // Check if anything changed
                var hasChanges = cliente.Nombre != clienteDto.Nombre
                    || cliente.Rfc != clienteDto.Rfc
                    || cliente.SaldoTotal != clienteDto.SaldoTotal
                    || cliente.SaldoVencido != clienteDto.SaldoVencido
                    || cliente.DiasMaxVencido != clienteDto.DiasMaxVencido;

                if (hasChanges)
                {
                    modificados++;
                }
                else
                {
                    sinCambios++;
                }
            }

            // Update client fields
            cliente.Nombre = clienteDto.Nombre;
            cliente.Rfc = clienteDto.Rfc;
            cliente.SaldoTotal = clienteDto.SaldoTotal;
            cliente.SaldoVencido = clienteDto.SaldoVencido;
            cliente.DiasMaxVencido = clienteDto.DiasMaxVencido;
            cliente.LastSyncAt = DateTime.UtcNow;
            cliente.UpdatedAt = DateTime.UtcNow;

            // Process invoices
            if (clienteDto.Facturas != null)
            {
                foreach (var facturaDto in clienteDto.Facturas)
                {
                    if (facturaDto.Operation == "delete")
                    {
                        continue;
                    }

                    var factura = cliente.Facturas.FirstOrDefault(f => f.Folio == facturaDto.Folio);

                    if (factura == null)
                    {
                        factura = new Factura
                        {
                            Id = Guid.NewGuid(),
                            Folio = facturaDto.Folio,
                            ClienteId = cliente.Id,
                            CreatedAt = DateTime.UtcNow
                        };
                        cliente.Facturas.Add(factura);
                    }

                    factura.Fecha = facturaDto.Fecha;
                    factura.Vencimiento = facturaDto.Vencimiento;
                    factura.Total = facturaDto.Total;
                    factura.Saldo = facturaDto.Saldo;
                    factura.DiasVencido = facturaDto.DiasVencido;
                    factura.Status = facturaDto.DiasVencido > 0 ? FacturaStatus.Vencida : FacturaStatus.Vigente;
                    factura.LastSyncAt = DateTime.UtcNow;
                    factura.UpdatedAt = DateTime.UtcNow;

                    facturasActualizadas++;
                }

                cliente.FacturasActivas = cliente.Facturas.Count(f => f.Status != FacturaStatus.Pagada && f.Status != FacturaStatus.Cancelada);
            }

            // Process contacts
            if (clienteDto.Contactos != null)
            {
                var existingContactos = await db.Contactos
                    .Where(c => c.ClienteId == cliente.Id)
                    .ToListAsync(ct);

                foreach (var contactoDto in clienteDto.Contactos)
                {
                    var contacto = existingContactos.FirstOrDefault(c => c.Nombre == contactoDto.Nombre);

                    if (contacto == null)
                    {
                        contacto = new Contacto
                        {
                            Id = Guid.NewGuid(),
                            Nombre = contactoDto.Nombre,
                            ClienteId = cliente.Id,
                            CreatedAt = DateTime.UtcNow
                        };
                        db.Contactos.Add(contacto);
                    }

                    contacto.Email = contactoDto.Email;
                    contacto.Telefono = contactoDto.Telefono;
                    contacto.UpdatedAt = DateTime.UtcNow;
                }
            }
        }

        // Update connector last sync timestamp
        var connector = await db.Connectors.FindAsync([connectorId], ct);
        if (connector != null)
        {
            connector.LastSyncAt = DateTime.UtcNow;
        }

        await db.SaveChangesAsync(ct);

        stats = new SyncStats(
            ClientesActualizados: nuevos + modificados,
            FacturasActualizadas: facturasActualizadas,
            Nuevos: nuevos,
            Modificados: modificados,
            SinCambios: sinCambios
        );

        logger.LogInformation(
            "Sync {SyncId} completed: {New} new, {Modified} modified, {Unchanged} unchanged, {Invoices} invoices",
            syncId, nuevos, modificados, sinCambios, facturasActualizadas);

        return Results.Ok(new SyncCarteraResponse(
            Success: true,
            SyncId: syncId,
            ProcessedAt: DateTime.UtcNow,
            Stats: stats
        ));
    }
}
