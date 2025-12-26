namespace CobranzaCloud.Application.Sync;

// ============================================================
// SYNC CARTERA REQUEST DTOs
// ============================================================

public record SyncCarteraRequest(
    string EmpresaId,
    string SyncType,
    DateTime Timestamp,
    string Checksum,
    SyncCarteraData Data
);

public record SyncCarteraData(
    CarteraResumen Resumen,
    List<RangoAntiguedadDto> Antiguedad,
    List<ClienteSyncDto> Clientes
);

public record CarteraResumen(
    decimal TotalCartera,
    decimal CarteraVigente,
    decimal CarteraVencida,
    int ClientesConSaldo,
    int FacturasActivas
);

public record RangoAntiguedadDto(
    string Rango,
    decimal Monto,
    int Facturas,
    decimal Porcentaje
);

public record ClienteSyncDto(
    string Operation,
    string Clave,
    string Nombre,
    string? Rfc,
    decimal SaldoTotal,
    decimal SaldoVencido,
    int DiasMaxVencido,
    List<FacturaSyncDto>? Facturas = null,
    List<ContactoSyncDto>? Contactos = null
);

public record FacturaSyncDto(
    string Operation,
    string Folio,
    DateTime Fecha,
    DateTime Vencimiento,
    decimal Total,
    decimal Saldo,
    int DiasVencido
);

public record ContactoSyncDto(
    string Nombre,
    string? Email = null,
    string? Telefono = null
);

// ============================================================
// SYNC CARTERA RESPONSE DTOs
// ============================================================

public record SyncCarteraResponse(
    bool Success,
    string SyncId,
    DateTime ProcessedAt,
    SyncStats Stats
);

public record SyncStats(
    int ClientesActualizados,
    int FacturasActualizadas,
    int Nuevos,
    int Modificados,
    int SinCambios
);
