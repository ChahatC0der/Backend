using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolERP.Domain.Fees.Entities;

namespace SchoolERP.Infrastructure.Persistence.Configurations.Fees;

public sealed class FinancialAuditLogConfiguration : IEntityTypeConfiguration<FinancialAuditLog>
{
    public void Configure(EntityTypeBuilder<FinancialAuditLog> builder)
    {
        builder.ToTable("FinancialAuditLogs");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.Property(x => x.TenantId);                    // nullable per BaseAuditLog
        builder.Property(x => x.PerformedBy).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");

        builder.Property(x => x.TableName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.RecordId).IsRequired();
        builder.Property(x => x.Action).HasMaxLength(30).IsRequired();
        builder.Property(x => x.OldValues).HasColumnType("nvarchar(max)");
        builder.Property(x => x.NewValues).HasColumnType("nvarchar(max)");

        builder.HasIndex(x => new { x.TenantId, x.TableName, x.RecordId })
            .HasDatabaseName("IX_FinancialAuditLogs_Tenant_Table_Record");

        builder.HasIndex(x => x.CreatedAt)
            .HasDatabaseName("IX_FinancialAuditLogs_CreatedAt");
    }
}
