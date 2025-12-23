namespace CobranzaCloud.Core.Entities;

/// <summary>
/// Represents a tenant organization in the system
/// </summary>
public class Organization
{
    public Guid Id { get; set; }
    public required string Nombre { get; set; }
    public string? Rfc { get; set; }
    public OrganizationPlan Plan { get; set; } = OrganizationPlan.Free;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Settings
    public string Timezone { get; set; } = "America/Mexico_City";
    public string Currency { get; set; } = "MXN";
    public string Locale { get; set; } = "es-MX";
    public string? EmailDomain { get; set; }

    // Navigation properties
    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<Connector> Connectors { get; set; } = new List<Connector>();
    public ICollection<Cliente> Clientes { get; set; } = new List<Cliente>();
}

public enum OrganizationPlan
{
    Free,
    Starter,
    Professional,
    Enterprise
}
