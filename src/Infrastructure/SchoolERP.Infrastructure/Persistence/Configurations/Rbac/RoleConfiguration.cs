using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolERP.Domain.Rbac.Entities;
using SchoolERP.Infrastructure.Persistence.Configurations.Common;

public class RoleConfiguration : BaseTenantEntityConfiguration<Role>
{
    public override void Configure(EntityTypeBuilder<Role> builder)
    {
        base.Configure(builder);
        builder.ToTable("Roles");
        builder.Property(r => r.Name).HasMaxLength(100).IsRequired();
        builder.Property(r => r.Code).HasMaxLength(100).IsRequired();
        builder.Property(r => r.Description).HasMaxLength(255);
        builder.Property(r => r.IsBuiltin).HasDefaultValue(false);
        builder.Property(r => r.IsSystem).HasDefaultValue(false);
        builder.HasOne(r => r.BaseRole).WithMany().HasForeignKey(r => r.BaseRoleId).OnDelete(DeleteBehavior.NoAction);
        builder.HasIndex(r => new { r.TenantId, r.Code }).IsUnique().HasFilter("[IsDeleted] = 0");
    }
}