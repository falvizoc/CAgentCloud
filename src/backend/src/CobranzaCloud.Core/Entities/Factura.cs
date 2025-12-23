namespace CobranzaCloud.Core.Entities;

/// <summary>
/// Represents an invoice in the collection system
/// </summary>
public class Factura
{
    public Guid Id { get; set; }
    public required string Folio { get; set; }
    public DateTime Fecha { get; set; }
    public DateTime Vencimiento { get; set; }
    public decimal Total { get; set; }
    public decimal Saldo { get; set; }
    public int DiasVencido { get; set; }
    public FacturaStatus Status { get; set; } = FacturaStatus.Vigente;

    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? LastSyncAt { get; set; }

    // Navigation
    public Guid ClienteId { get; set; }
    public Cliente Cliente { get; set; } = null!;

    // Calculated property
    public RangoAntiguedad RangoAntiguedad => DiasVencido switch
    {
        <= 0 => RangoAntiguedad.Vigente,
        <= 30 => RangoAntiguedad.Dias1a30,
        <= 60 => RangoAntiguedad.Dias31a60,
        <= 90 => RangoAntiguedad.Dias61a90,
        _ => RangoAntiguedad.MasDe90
    };
}

public enum FacturaStatus
{
    Vigente,
    Vencida,
    Pagada,
    Cancelada
}

public enum RangoAntiguedad
{
    Vigente,
    Dias1a30,
    Dias31a60,
    Dias61a90,
    MasDe90
}
