using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolERP.Domain.Rbac.Entities;
using SchoolERP.Infrastructure.Persistence.Configurations.Common;

public class RbacAuditLogConfiguration : BaseTenantEntityConfiguration<RbacAuditLog>
{
    public override void Configure(EntityTypeBuilder<RbacAuditLog> builder)
    {
        base.Configure(builder);
        builder.ToTable("RbacAuditLogs");
        builder.Property(ral => ral.PerformedBy).IsRequired();
        builder.Property(ral => ral.AffectedUserId).IsRequired(false);
        builder.Property(ral => ral.AffectedRoleId).IsRequired(false);
        builder.Property(ral => ral.Action).HasMaxLength(100).IsRequired();
        builder.HasOne(ral => ral.PerformedByUser).WithMany(u => u.RbacAuditLogsPerformed).HasForeignKey(ral => ral.PerformedBy).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(ral => ral.AffectedUser).WithMany(u => u.RbacAuditLogsAffected).HasForeignKey(ral => ral.AffectedUserId).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(ral => ral.AffectedRole).WithMany().HasForeignKey(ral => ral.AffectedRoleId).OnDelete(DeleteBehavior.NoAction);
        builder.HasIndex(ral => new { ral.TenantId, ral.CreatedAt });
        builder.HasIndex(ral => ral.PerformedBy);
    }
}