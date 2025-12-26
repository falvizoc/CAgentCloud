namespace CobranzaCloud.Core.Entities;

/// <summary>
/// Represents a temporary link code for secure connector registration.
/// Code is 6 alphanumeric characters, expires in 15 minutes, single-use.
/// </summary>
public class LinkCode
{
    public Guid Id { get; set; }

    /// <summary>
    /// 6-character alphanumeric code (uppercase)
    /// </summary>
    public required string Code { get; set; }

    public DateTime ExpiresAt { get; set; }
    public bool Used { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Metadata from the connector requesting the code
    public required string MachineFingerprint { get; set; }
    public required string ConnectorName { get; set; }
    public required string ConnectorVersion { get; set; }

    // Multi-tenant
    public Guid OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;

    // User who generated the code
    public Guid CreatedByUserId { get; set; }
    public User CreatedBy { get; set; } = null!;
}
