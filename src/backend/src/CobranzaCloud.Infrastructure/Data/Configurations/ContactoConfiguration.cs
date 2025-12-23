using CobranzaCloud.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CobranzaCloud.Infrastructure.Data.Configurations;

public class ContactoConfiguration : IEntityTypeConfiguration<Contacto>
{
    public void Configure(EntityTypeBuilder<Contacto> builder)
    {
        builder.ToTable("contactos");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Nombre)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(c => c.Email)
            .HasMaxLength(255);

        builder.Property(c => c.Telefono)
            .HasMaxLength(20);

        // Relationship
        builder.HasOne(c => c.Cliente)
            .WithMany(cl => cl.Contactos)
            .HasForeignKey(c => c.ClienteId)
            .OnDelete(DeleteBehavior.Cascade);

        // Index
        builder.HasIndex(c => c.ClienteId);
    }
}
