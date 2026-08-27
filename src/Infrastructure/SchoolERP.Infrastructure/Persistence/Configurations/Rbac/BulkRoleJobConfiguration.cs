using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolERP.Domain.Rbac.Entities;
using SchoolERP.Infrastructure.Persistence.Configurations.Common;

public class BulkRoleJobConfiguration : BaseTenantEntityConfiguration<BulkRoleJob>
{
    public override void Configure(EntityTypeBuilder<BulkRoleJob> builder)
    {
        base.Configure(builder);
        builder.ToTable("BulkRoleJobs");
        builder.Property(brj => brj.CreatedBy).IsRequired();
        builder.Property(brj => brj.RoleId).IsRequired();
        builder.Property(brj => brj.ScopeType).HasMaxLength(50).IsRequired();
        builder.Property(brj => brj.ScopeValue).HasMaxLength(50).IsRequired();
        builder.Property(brj => brj.TotalUsers).IsRequired();
        builder.Property(brj => brj.ProcessedCount).HasDefaultValue(0);
        builder.Property(brj => brj.FailedCount).HasDefaultValue(0);
        builder.Property(brj => brj.Status).HasMaxLength(50).HasDefaultValue("pending");
        builder.HasOne(brj => brj.RequestedBy).WithMany(u => u.BulkRoleJobs).HasForeignKey(brj => brj.CreatedBy).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(brj => brj.Role).WithMany().HasForeignKey(brj => brj.RoleId).OnDelete(DeleteBehavior.NoAction);
        builder.HasIndex(brj => brj.Status);
    }
}