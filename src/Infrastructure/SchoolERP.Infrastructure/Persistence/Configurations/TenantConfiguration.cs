using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolERP.Domain.Tenants.Entities;

namespace SchoolERP.Infrastructure.Persistence.Configurations;

public class TenantConfiguration : BaseGuidEntityConfiguration<Tenant>
{
    public override void Configure(EntityTypeBuilder<Tenant> builder)
    {
        base.Configure(builder);

        builder.ToTable("Tenants");

        // 🔥 Identity & Branding
        builder.Property(e => e.Code).IsRequired().HasMaxLength(50);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(255);
        builder.Property(e => e.Subdomain).IsRequired().HasMaxLength(100);
        builder.Property(e => e.LogoUrl).HasMaxLength(500);
        builder.Property(e => e.FaviconUrl).HasMaxLength(500);

        // 🔥 Contact
        builder.Property(e => e.ContactEmail).IsRequired().HasMaxLength(255);
        builder.Property(e => e.ContactPhone).HasMaxLength(20);
        builder.Property(e => e.Address).HasColumnType("nvarchar(max)");

        // 🔥 Owner
        builder.Property(e => e.OwnerName).HasMaxLength(255);
        builder.Property(e => e.OwnerEmail).HasMaxLength(255);
        builder.Property(e => e.OwnerPhone).HasMaxLength(20);
        builder.Property(e => e.OwnerDesignation).HasMaxLength(100);

        // 🔥 Legal
        builder.Property(e => e.Gstin).HasMaxLength(50);
        builder.Property(e => e.Pan).HasMaxLength(50);
        builder.Property(e => e.RegistrationNumber).HasMaxLength(100);
        builder.Property(e => e.Affiliation).HasMaxLength(100);

        // 🔥 Localization
        builder.Property(e => e.Currency).HasMaxLength(10).HasDefaultValue("INR");
        builder.Property(e => e.TimeZone).HasMaxLength(100).HasDefaultValue("India Standard Time");
        builder.Property(e => e.Plan).HasMaxLength(20).HasDefaultValue("basic");
        builder.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("active");

        // 🔥 JSON
        builder.Property(e => e.Settings).HasColumnType("nvarchar(max)").HasDefaultValue("{}");
        builder.Property(e => e.CustomFieldsDef).HasColumnType("nvarchar(max)").HasDefaultValue("{}");

        // 🔥 Unique Constraints
        builder.HasIndex(e => e.Subdomain).IsUnique();
        builder.HasIndex(e => e.Code).IsUnique();
        builder.HasIndex(e => e.ContactEmail).IsUnique();
    }
}