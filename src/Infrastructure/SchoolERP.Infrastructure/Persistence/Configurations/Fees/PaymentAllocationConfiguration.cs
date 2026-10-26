using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolERP.Domain.Fees.Entities;

namespace SchoolERP.Infrastructure.Persistence.Configurations.Fees;

public sealed class PaymentAllocationConfiguration : IEntityTypeConfiguration<PaymentAllocation>
{
    public void Configure(EntityTypeBuilder<PaymentAllocation> builder)
    {
        builder.ToTable("PaymentAllocations");
        builder.HasKey(x => new { x.PaymentId, x.InvoiceId });

        builder.Property(x => x.PaymentId).IsRequired();
        builder.Property(x => x.InvoiceId).IsRequired();
        builder.Property(x => x.AllocatedAmount).HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasIndex(x => x.InvoiceId).HasDatabaseName("IX_PaymentAllocations_InvoiceId");
    }
}
