using CobranzaCloud.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CobranzaCloud.Infrastructure.Data.Configurations;

public class ConnectorConfiguration : IEntityTypeConfiguration<Connector>
{
    public void Configure(EntityTypeBuilder<Connector> builder)
    {
        builder.ToTable("connectors");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Nombre)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(c => c.Version)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(c => c.MachineFingerprint)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(c => c.Tipo)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(c => c.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(ConnectorStatus.Pending);

        // Relationship
        builder.HasOne(c => c.Organization)
            .WithMany(o => o.Connectors)
            .HasForeignKey(c => c.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        // Index
        builder.HasIndex(c => c.OrganizationId);
    }
}
