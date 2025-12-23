using CobranzaCloud.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CobranzaCloud.Infrastructure.Data.Configurations;

public class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
{
    public void Configure(EntityTypeBuilder<Organization> builder)
    {
        builder.ToTable("organizations");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Nombre)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(o => o.Rfc)
            .HasMaxLength(13);

        builder.Property(o => o.Plan)
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(OrganizationPlan.Free);

        builder.Property(o => o.Timezone)
            .HasMaxLength(50)
            .HasDefaultValue("America/Mexico_City");

        builder.Property(o => o.Currency)
            .HasMaxLength(3)
            .HasDefaultValue("MXN");

        builder.Property(o => o.Locale)
            .HasMaxLength(10)
            .HasDefaultValue("es-MX");

        builder.Property(o => o.EmailDomain)
            .HasMaxLength(255);

        builder.HasIndex(o => o.Nombre);
    }
}
