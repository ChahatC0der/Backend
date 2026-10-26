using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolERP.Domain.Fees.Entities;
using SchoolERP.Domain.Fees.Enums;
using SchoolERP.Infrastructure.Persistence.Converters;

namespace SchoolERP.Infrastructure.Persistence.Configurations.Fees;

public sealed class RefundConfiguration : IEntityTypeConfiguration<Refund>
{
    public void Configure(EntityTypeBuilder<Refund> builder)
    {
        builder.ToTable("Refunds");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        // Physical dbo.Refunds has no IsDeleted and no DeletedAt
        builder.Ignore(x => x.IsDeleted);
        builder.Ignore(x => x.DeletedAt);

        builder.Property(x => x.TenantId).IsRequired();
        builder.Property(x => x.StudentId).IsRequired();
        builder.Property(x => x.PaymentId);
        builder.Property(x => x.Amount).HasPrecision(18, 2).IsRequired();

        builder.Property(x => x.Reason)
            .HasConversion(new EnumStringConverter<RefundReason>(EnumMaps.RefundReasonMap))
            .HasMaxLength(30).IsRequired();

        builder.Property(x => x.PaymentMode)
            .HasConversion(new EnumStringConverter<PaymentMode>(EnumMaps.PaymentModeMap))
            .HasMaxLength(20).IsRequired();

        builder.Property(x => x.ReferenceNumber).HasMaxLength(100);

        builder.Property(x => x.Status)
            .HasConversion(new EnumStringConverter<RefundStatus>(EnumMaps.RefundStatusMap))
            .HasMaxLength(20).IsRequired();

        builder.Property(x => x.ApprovedBy);
        builder.Property(x => x.ApprovedAt);
        builder.Property(x => x.RejectionReason).HasMaxLength(500);
        builder.Property(x => x.CreatedAt).IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property(x => x.UpdatedAt);

        builder.HasIndex(x => new { x.TenantId, x.StudentId, x.Status })
            .HasDatabaseName("IX_Refunds_Tenant_Student_Status");

        builder.HasIndex(x => x.PaymentId)
            .HasDatabaseName("IX_Refunds_PaymentId")
            .HasFilter("[PaymentId] IS NOT NULL");
    }
}
