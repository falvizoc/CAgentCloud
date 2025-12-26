using CobranzaCloud.Core.Entities;

namespace CobranzaCloud.Application.Connectors;

// ============================================================
// LINK CODE DTOs
// ============================================================

public record GenerateLinkCodeRequest(
    string ConnectorName,
    string ConnectorVersion,
    string MachineFingerprint
);

public record LinkCodeResponse(
    string Code,
    DateTime ExpiresAt
);

// ============================================================
// CONNECTOR REGISTRATION DTOs
// ============================================================

public record RegisterConnectorRequest(
    string LinkCode,
    string MachineFingerprint,
    string ConnectorName,
    string ConnectorVersion,
    ConnectorType Tipo,
    List<EmpresaInfo>? Empresas = null
);

public record EmpresaInfo(
    string Id,
    string Nombre,
    string? BaseDatos = null
);

public record RegisterConnectorResponse(
    Guid ConnectorId,
    string AccessToken,
    string RefreshToken,
    int ExpiresIn,
    SyncConfig Config
);

public record SyncConfig(
    int SyncIntervalMinutes,
    int HeartbeatIntervalMinutes
);

// ============================================================
// HEARTBEAT DTOs
// ============================================================

public record HeartbeatRequest(
    string Status,
    long Uptime,
    double MemoryUsageMb,
    string? LastSyncStatus = null,
    List<string>? EmpresasOnline = null
);

public record HeartbeatResponse(
    bool Ack,
    DateTime ServerTime,
    List<ConnectorCommand>? Commands = null
);

public record ConnectorCommand(
    string Type,
    object? Payload = null
);

// ============================================================
// CONNECTOR QUERY DTOs
// ============================================================

public record ConnectorSummaryResponse(
    Guid Id,
    string Nombre,
    string Tipo,
    string Status,
    string Version,
    DateTime? LastHeartbeat,
    DateTime? LastSyncAt,
    DateTime CreatedAt
);
