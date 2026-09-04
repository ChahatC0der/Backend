using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolERP.Domain.Rbac.Entities;

namespace SchoolERP.Infrastructure.Persistence.Configurations.Rbac;

public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("Permissions");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedOnAdd();

        builder.Property(p => p.ModuleId).IsRequired();
        builder.Property(p => p.Action).HasMaxLength(50).IsRequired();
        builder.Property(p => p.Key).HasMaxLength(255).IsRequired();
        builder.Property(p => p.Description).HasMaxLength(255); // optional

        // Relationship
        builder.HasOne(p => p.Module)
            .WithMany(m => m.Permissions)
            .HasForeignKey(p => p.ModuleId)
            .OnDelete(DeleteBehavior.Cascade);

        // Unique index on Key
        builder.HasIndex(p => p.Key).IsUnique();

        // Index on ModuleId
        builder.HasIndex(p => p.ModuleId);
    }
}