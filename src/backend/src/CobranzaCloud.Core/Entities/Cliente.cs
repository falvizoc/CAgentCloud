namespace CobranzaCloud.Core.Entities;

/// <summary>
/// Represents a customer (client) in the collection system
/// </summary>
public class Cliente
{
    public Guid Id { get; set; }
    public required string Clave { get; set; }
    public required string Nombre { get; set; }
    public string? Rfc { get; set; }

    // Contact
    public string? Email { get; set; }
    public string? Telefono { get; set; }

    // Address
    public string? Calle { get; set; }
    public string? Colonia { get; set; }
    public string? Ciudad { get; set; }
    public string? Estado { get; set; }
    public string? CodigoPostal { get; set; }

    // Balance (updated by sync)
    public decimal SaldoTotal { get; set; }
    public decimal SaldoVencido { get; set; }
    public int DiasMaxVencido { get; set; }
    public int FacturasActivas { get; set; }
    public DateTime? UltimoPago { get; set; }

    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? LastSyncAt { get; set; }

    // Organization (required - multi-tenant)
    public Guid OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;

    // Navigation
    public ICollection<Factura> Facturas { get; set; } = new List<Factura>();
    public ICollection<Contacto> Contactos { get; set; } = new List<Contacto>();
}
