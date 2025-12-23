namespace CobranzaCloud.Core.Entities;

/// <summary>
/// Represents a contact for a customer
/// </summary>
public class Contacto
{
    public Guid Id { get; set; }
    public required string Nombre { get; set; }
    public string? Email { get; set; }
    public string? Telefono { get; set; }
    public bool Principal { get; set; }

    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public Guid ClienteId { get; set; }
    public Cliente Cliente { get; set; } = null!;
}
