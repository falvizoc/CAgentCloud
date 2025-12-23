using CobranzaCloud.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CobranzaCloud.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Email)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(u => u.Nombre)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(u => u.PasswordHash)
            .HasMaxLength(255);

        builder.Property(u => u.Role)
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(UserRole.Viewer);

        builder.Property(u => u.AuthProvider)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(u => u.GoogleId)
            .HasMaxLength(255);

        builder.Property(u => u.MicrosoftId)
            .HasMaxLength(255);

        // Unique email per organization
        builder.HasIndex(u => new { u.OrganizationId, u.Email })
            .IsUnique();

        // Index for OAuth lookups
        builder.HasIndex(u => u.GoogleId)
            .HasFilter("\"GoogleId\" IS NOT NULL");

        builder.HasIndex(u => u.MicrosoftId)
            .HasFilter("\"MicrosoftId\" IS NOT NULL");

        // Relationship
        builder.HasOne(u => u.Organization)
            .WithMany(o => o.Users)
            .HasForeignKey(u => u.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
