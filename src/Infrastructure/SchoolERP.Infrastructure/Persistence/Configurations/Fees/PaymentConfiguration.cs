using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolERP.Domain.Fees.Entities;
using SchoolERP.Domain.Fees.Enums;
using SchoolERP.Infrastructure.Persistence.Converters;

namespace SchoolERP.Infrastructure.Persistence.Configurations.Fees;

public sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.Ignore(x => x.IsDeleted);
        builder.Property(x => x.TenantId).IsRequired();
        builder.Property(x => x.BranchId).IsRequired();

        builder.Property(x => x.StudentId).IsRequired();
        builder.Property(x => x.ReceiptNumber).HasMaxLength(50).IsRequired();
        builder.Property(x => x.PaymentDate).IsRequired();

        builder.Property(x => x.Amount).HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.AllocatedAmount).HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.UnallocatedAmount).HasPrecision(18, 2).IsRequired();

        builder.Property(x => x.PaymentSource)
            .HasConversion(new EnumStringConverter<PaymentSource>(EnumMaps.PaymentSourceMap))
            .HasMaxLength(20).IsRequired();

        builder.Property(x => x.Mode)
            .HasConversion(new EnumStringConverter<PaymentMode>(EnumMaps.PaymentModeMap))
            .HasMaxLength(20).IsRequired();

        builder.Property(x => x.ReferenceNumber).HasMaxLength(100);
        builder.Property(x => x.Notes).HasMaxLength(1000);
        builder.Property(x => x.CollectedBy);

        builder.Property(x => x.Status)
            .HasConversion(new EnumStringConverter<PaymentStatus>(EnumMaps.PaymentStatusMap))
            .HasMaxLength(20).IsRequired();

        builder.Property(x => x.CreatedAt).IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property(x => x.UpdatedAt);
        builder.Property(x => x.DeletedAt);

        builder.HasMany(x => x.Allocations)
            .WithOne()
            .HasForeignKey(a => a.PaymentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.TenantId, x.BranchId, x.ReceiptNumber })
            .HasDatabaseName("UQ_Payments_Tenant_Branch_ReceiptNumber")
            .IsUnique();

        builder.HasIndex(x => new { x.StudentId, x.PaymentDate })
            .HasDatabaseName("IX_Payments_StudentId_PaymentDate")
            .IsDescending(false, true);
    }
}
