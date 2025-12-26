namespace CobranzaCloud.Application.Cartera;

/// <summary>
/// Response for GET /api/cartera/resumen
/// </summary>
public record CarteraResumenResponse(
    decimal TotalCartera,
    decimal CarteraVigente,
    decimal CarteraVencida,
    decimal PorcentajeVencido,
    int ClientesConSaldo,
    int FacturasActivas,
    DateTime? UltimaSincronizacion
);

/// <summary>
/// Response for GET /api/cartera/antiguedad
/// </summary>
public record CarteraAntiguedadResponse(
    List<RangoAntiguedadItem> Rangos,
    decimal Total
);

/// <summary>
/// Individual range item for aging report
/// </summary>
public record RangoAntiguedadItem(
    string Rango,
    string Label,
    decimal Monto,
    int Facturas,
    decimal Porcentaje
);

/// <summary>
/// Client list item for GET /api/clientes
/// </summary>
public record ClienteListItem(
    Guid Id,
    string Clave,
    string Nombre,
    decimal SaldoTotal,
    decimal SaldoVencido,
    int DiasMaxVencido,
    int FacturasActivas
);

/// <summary>
/// Response for GET /api/clientes with pagination
/// </summary>
public record ClientesListResponse(
    List<ClienteListItem> Items,
    PaginationMeta Meta
);

/// <summary>
/// Pagination metadata
/// </summary>
public record PaginationMeta(
    int Page,
    int PageSize,
    int Total,
    int TotalPages
);
