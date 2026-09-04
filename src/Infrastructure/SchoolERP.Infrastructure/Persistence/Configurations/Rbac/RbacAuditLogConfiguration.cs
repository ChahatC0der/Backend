using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolERP.Domain.Rbac.Entities;
using SchoolERP.Infrastructure.Persistence.Configurations.Common;

namespace SchoolERP.Infrastructure.Persistence.Configurations.Rbac;

public class RbacAuditLogConfiguration : BaseAuditLogConfiguration<RbacAuditLog>
{
    public override void Configure(EntityTypeBuilder<RbacAuditLog> builder)
    {
        base.Configure(builder); // TenantId, soft delete, timestamps

        builder.ToTable("RbacAuditLogs");

        builder.Property(ral => ral.PerformedBy)
            .IsRequired();

        builder.Property(ral => ral.AffectedUserId)
            .IsRequired(false);

        builder.Property(ral => ral.AffectedRoleId)
            .IsRequired(false);

        builder.Property(ral => ral.Resource)
            .HasMaxLength(100)
            .IsRequired(false);
        builder.Property(ral => ral.Action)
            .HasMaxLength(100)
            .IsRequired();

        // JSON-like columns (store as nvarchar(max))
        builder.Property(ral => ral.OldValues)
            .HasColumnType("nvarchar(max)");

        builder.Property(ral => ral.NewValues)
            .HasColumnType("nvarchar(max)");

        builder.Property(ral => ral.Reason)
            .HasColumnType("nvarchar(max)");

        // Relationships
        builder.HasOne(ral => ral.PerformedByUser)
            .WithMany(u => u.RbacAuditLogsPerformed)
            .HasForeignKey(ral => ral.PerformedBy)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(ral => ral.AffectedUser)
            .WithMany(u => u.RbacAuditLogsAffected)
            .HasForeignKey(ral => ral.AffectedUserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(ral => ral.AffectedRole)
            .WithMany() // no navigation on Role side
            .HasForeignKey(ral => ral.AffectedRoleId)
            .OnDelete(DeleteBehavior.NoAction);

        // Indexes
        builder.HasIndex(ral => new { ral.TenantId, ral.CreatedAt });
        builder.HasIndex(ral => ral.PerformedBy);
        builder.HasIndex(ral => ral.AffectedUserId);
        builder.HasIndex(ral => ral.AffectedRoleId);
        builder.HasIndex(ral => ral.Action);
    }
}