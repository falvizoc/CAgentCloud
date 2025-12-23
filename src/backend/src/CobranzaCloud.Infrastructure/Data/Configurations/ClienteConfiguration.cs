using CobranzaCloud.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CobranzaCloud.Infrastructure.Data.Configurations;

public class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> builder)
    {
        builder.ToTable("clientes");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Clave)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(c => c.Nombre)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(c => c.Rfc)
            .HasMaxLength(13);

        builder.Property(c => c.Email)
            .HasMaxLength(255);

        builder.Property(c => c.Telefono)
            .HasMaxLength(20);

        builder.Property(c => c.Calle)
            .HasMaxLength(255);

        builder.Property(c => c.Colonia)
            .HasMaxLength(100);

        builder.Property(c => c.Ciudad)
            .HasMaxLength(100);

        builder.Property(c => c.Estado)
            .HasMaxLength(100);

        builder.Property(c => c.CodigoPostal)
            .HasMaxLength(10);

        builder.Property(c => c.SaldoTotal)
            .HasPrecision(18, 2);

        builder.Property(c => c.SaldoVencido)
            .HasPrecision(18, 2);

        // Unique clave per organization
        builder.HasIndex(c => new { c.OrganizationId, c.Clave })
            .IsUnique();

        // Relationship
        builder.HasOne(c => c.Organization)
            .WithMany(o => o.Clientes)
            .HasForeignKey(c => c.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
