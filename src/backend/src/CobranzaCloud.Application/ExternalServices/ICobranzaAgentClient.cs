namespace CobranzaCloud.Application.ExternalServices;

/// <summary>
/// Client interface for Cobranza Agent API (ASPEL Connector)
/// </summary>
public interface ICobranzaAgentClient
{
    /// <summary>
    /// Check if the connector is healthy
    /// </summary>
    Task<AgentHealthResponse?> GetHealthAsync(CancellationToken ct = default);

    /// <summary>
    /// Get list of configured empresas
    /// </summary>
    Task<AgentResponse<List<AgentEmpresa>>?> GetEmpresasAsync(CancellationToken ct = default);

    /// <summary>
    /// Get cartera summary for an empresa
    /// </summary>
    /// <param name="empresaId">Empresa ID (e.g., "01")</param>
    /// <param name="moneda">Currency code: 1=MXN, 2=USD. Default: 1 (MXN)</param>
    Task<AgentResponse<AgentCarteraResumen>?> GetCarteraResumenAsync(
        string empresaId,
        int moneda = 1,
        CancellationToken ct = default);

    /// <summary>
    /// Get aging analysis for an empresa
    /// </summary>
    Task<AgentResponse<AgentCarteraAntiguedad>?> GetCarteraAntiguedadAsync(
        string empresaId,
        int moneda = 1,
        CancellationToken ct = default);

    /// <summary>
    /// Get clients with balance
    /// </summary>
    Task<AgentPaginatedResponse<AgentCliente>?> GetClientesAsync(
        string empresaId,
        int limite = 50,
        int offset = 0,
        CancellationToken ct = default);

    /// <summary>
    /// Get client detail by clave
    /// </summary>
    Task<AgentResponse<AgentClienteDetalle>?> GetClienteDetalleAsync(
        string empresaId,
        string claveCliente,
        CancellationToken ct = default);

    /// <summary>
    /// Get overdue invoices
    /// </summary>
    Task<AgentPaginatedResponse<AgentFactura>?> GetCarteraVencidaAsync(
        string empresaId,
        int moneda = 1,
        int limite = 50,
        int offset = 0,
        CancellationToken ct = default);
}

// ============================================================
// RESPONSE WRAPPERS
// ============================================================

public record AgentResponse<T>(
    bool Success,
    T? Data,
    AgentError? Error,
    string? Message
);

public record AgentPaginatedResponse<T>(
    bool Success,
    List<T>? Items,
    AgentPagination? Pagination,
    AgentError? Error
);

public record AgentError(string Code, string Message);

public record AgentPagination(
    int Total,
    int Limit,
    int Offset,
    int Page,
    int TotalPages
);

public record AgentHealthResponse(
    string Status,
    DateTime Timestamp,
    string Version,
    string Service
);

// ============================================================
// DATA MODELS
// ============================================================

public record AgentEmpresa(
    string Id,
    string? Rfc,
    string Motor,
    bool Activa,
    string EstadoConexion,
    string? RutaBaseDatos,
    string? MensajeError
);

public record AgentCarteraResumen(
    decimal TotalCartera,
    decimal CarteraVencida,
    decimal CarteraPorVencer,
    int TotalFacturas,
    int FacturasVencidas,
    int ClientesConSaldo,
    string Moneda
);

public record AgentCarteraAntiguedad(
    decimal Corriente,
    decimal Rango1a30,
    decimal Rango31a60,
    decimal Rango61a90,
    decimal RangoMas90,
    decimal Total,
    string Moneda,
    List<AgentRangoAntiguedad>? Rangos
);

public record AgentRangoAntiguedad(
    string Nombre,
    int DiasDesde,
    int? DiasHasta,
    decimal Monto
);

public record AgentCliente(
    string Clave,
    string Nombre,
    string? Rfc,
    string? Email,
    decimal SaldoTotal,
    decimal SaldoVencido,
    int FacturasPendientes
);

public record AgentClienteDetalle(
    string Clave,
    string Nombre,
    string? Rfc,
    string? Email,
    string? Telefono,
    string? Celular,
    string? Contacto,
    decimal LimiteCredito,
    int DiasCredito,
    string? Status,
    List<string>? Emails
);

public record AgentFactura(
    string ClaveCliente,
    string NoFactura,
    decimal Cargo,
    decimal Pagado,
    decimal Saldo,
    DateTime FechaFactura,
    DateTime FechaVencimiento,
    int DiasVencido,
    bool EstaVencida,
    int Moneda,
    string MonedaDescripcion,
    string? Referencia
);
