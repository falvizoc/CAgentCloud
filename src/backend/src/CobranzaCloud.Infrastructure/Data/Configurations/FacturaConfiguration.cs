using CobranzaCloud.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CobranzaCloud.Infrastructure.Data.Configurations;

public class FacturaConfiguration : IEntityTypeConfiguration<Factura>
{
    public void Configure(EntityTypeBuilder<Factura> builder)
    {
        builder.ToTable("facturas");

        builder.HasKey(f => f.Id);

        builder.Property(f => f.Folio)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(f => f.Total)
            .HasPrecision(18, 2);

        builder.Property(f => f.Saldo)
            .HasPrecision(18, 2);

        builder.Property(f => f.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(FacturaStatus.Vigente);

        // Ignore computed property
        builder.Ignore(f => f.RangoAntiguedad);

        // Unique folio per cliente
        builder.HasIndex(f => new { f.ClienteId, f.Folio })
            .IsUnique();

        // Relationship
        builder.HasOne(f => f.Cliente)
            .WithMany(c => c.Facturas)
            .HasForeignKey(f => f.ClienteId)
            .OnDelete(DeleteBehavior.Cascade);

        // Index for queries
        builder.HasIndex(f => f.Vencimiento);
        builder.HasIndex(f => f.Status);
    }
}
