using CobranzaCloud.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CobranzaCloud.Infrastructure.Data.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refresh_tokens");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Token)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(t => t.CreatedByIp)
            .HasMaxLength(45); // IPv6 max length

        builder.Property(t => t.RevokedByIp)
            .HasMaxLength(45);

        builder.Property(t => t.ReplacedByToken)
            .HasMaxLength(500);

        builder.Property(t => t.ReasonRevoked)
            .HasMaxLength(255);

        // Index for token lookup
        builder.HasIndex(t => t.Token);

        // Relationship
        builder.HasOne(t => t.User)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
