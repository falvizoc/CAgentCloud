using CobranzaCloud.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CobranzaCloud.Infrastructure.Data.Configurations;

public class LinkCodeConfiguration : IEntityTypeConfiguration<LinkCode>
{
    public void Configure(EntityTypeBuilder<LinkCode> builder)
    {
        builder.ToTable("link_codes");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Code)
            .HasMaxLength(6)
            .IsRequired();

        builder.Property(l => l.MachineFingerprint)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(l => l.ConnectorName)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(l => l.ConnectorVersion)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(l => l.Used)
            .HasDefaultValue(false);

        // Relationships
        builder.HasOne(l => l.Organization)
            .WithMany()
            .HasForeignKey(l => l.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(l => l.CreatedBy)
            .WithMany()
            .HasForeignKey(l => l.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(l => l.Code);
        builder.HasIndex(l => l.OrganizationId);
        builder.HasIndex(l => new { l.Code, l.Used, l.ExpiresAt });
    }
}
