using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolERP.Domain.Rbac.Entities;
using SchoolERP.Infrastructure.Persistence.Configurations.Common;

public class PermissionConfiguration : BaseEntityConfiguration<Permission>
{
    public override void Configure(EntityTypeBuilder<Permission> builder)
    {
        base.Configure(builder);
        builder.ToTable("Permissions");
        builder.Property(p => p.Action).HasMaxLength(50).IsRequired();
        builder.Property(p => p.Key).HasMaxLength(255).IsRequired();
        builder.Property(p => p.Description).HasMaxLength(255);
        builder.HasOne(p => p.Module).WithMany(m => m.Permissions).HasForeignKey(p => p.ModuleId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(p => p.Key).IsUnique();
        builder.HasIndex(p => p.ModuleId);
    }
}