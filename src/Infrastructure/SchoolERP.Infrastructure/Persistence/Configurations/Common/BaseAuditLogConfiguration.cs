using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolERP.Domain.Common; // BaseAuditLog

namespace SchoolERP.Infrastructure.Persistence.Configurations.Common;

/// <summary>
/// Base configuration for all audit log entities inheriting from BaseAuditLog.
/// Configures primary key, required fields, and common indexes.
/// </summary>
public abstract class BaseAuditLogConfiguration<TEntity> : IEntityTypeConfiguration<TEntity>
    where TEntity : BaseAuditLog
{
    public virtual void Configure(EntityTypeBuilder<TEntity> builder)
    {
        // Primary Key
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        // TenantId: required for tenant-scoped audit logs.
        // Agar kisi specific audit log me nullable chahiye to derived config me override kar sakte ho.
        builder.Property(x => x.TenantId)
            .IsRequired();

        // PerformedBy: user id who performed the action
        builder.Property(x => x.PerformedBy)
            .IsRequired();

        // CreatedAt: timestamp, default to UTC now if not set
        builder.Property(x => x.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        // Common indexes
        builder.HasIndex(x => x.TenantId);
        builder.HasIndex(x => new { x.TenantId, x.CreatedAt }); // efficient filtering by tenant+date
        builder.HasIndex(x => x.PerformedBy);
    }
}