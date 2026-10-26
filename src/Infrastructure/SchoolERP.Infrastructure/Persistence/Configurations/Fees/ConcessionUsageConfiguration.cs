using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolERP.Domain.Fees.Entities;

namespace SchoolERP.Infrastructure.Persistence.Configurations.Fees;

public sealed class ConcessionUsageConfiguration : IEntityTypeConfiguration<ConcessionUsage>
{
    public void Configure(EntityTypeBuilder<ConcessionUsage> builder)
    {
        builder.ToTable("ConcessionUsages");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.Property(x => x.ConcessionAllocationId).IsRequired();
        builder.Property(x => x.InvoiceId).IsRequired();
        builder.Property(x => x.AppliedAmount).HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasIndex(x => x.InvoiceId).HasDatabaseName("IX_ConcessionUsages_Invoice");
        builder.HasIndex(x => x.ConcessionAllocationId).HasDatabaseName("IX_ConcessionUsages_Allocation");
    }
}
