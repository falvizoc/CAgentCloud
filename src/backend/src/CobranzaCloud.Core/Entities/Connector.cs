namespace CobranzaCloud.Core.Entities;

/// <summary>
/// Represents a connector (local agent) that syncs data from ERP
/// </summary>
public class Connector
{
    public Guid Id { get; set; }
    public required string Nombre { get; set; }
    public ConnectorType Tipo { get; set; } = ConnectorType.AspelSae;
    public required string Version { get; set; }
    public ConnectorStatus Status { get; set; } = ConnectorStatus.Pending;
    public required string MachineFingerprint { get; set; }

    // Timestamps
    public DateTime? LastHeartbeat { get; set; }
    public DateTime? LastSyncAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Organization (required - multi-tenant)
    public Guid OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;
}

public enum ConnectorType
{
    AspelSae,
    Contpaqi,
    Custom
}

public enum ConnectorStatus
{
    Pending,
    Online,
    Offline,
    Error
}
