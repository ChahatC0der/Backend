using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolERP.Infrastructure.MultiTenancy;

namespace SchoolERP.Infrastructure.Persistence.Configurations;

public class AppTenantInfoConfiguration : IEntityTypeConfiguration<AppTenantInfo>
{
    public void Configure(EntityTypeBuilder<AppTenantInfo> builder)
    {
        // ✅ Map to existing Tenants table
        builder.ToTable("Tenants");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasMaxLength(36);  // Guid as string

        // ✅ Identifier ko Code column se map karo (jo already hai!)
        builder.Property(t => t.Identifier)
            .HasColumnName("Code")  // ← YEH KEY LINE HAI!
            .IsRequired()
            .HasMaxLength(50);

        // ✅ Name ko Name column se map karo
        builder.Property(t => t.Name)
            .HasColumnName("Name")
            .IsRequired()
            .HasMaxLength(255);

        // ✅ ConnectionString optional (common DB ke liye null)
        builder.Property(t => t.ConnectionString)
            .HasColumnName("ConnectionString")
            .HasMaxLength(1000);

        // ✅ Index on Identifier (Code)
        builder.HasIndex(t => t.Identifier).IsUnique();
    }
}