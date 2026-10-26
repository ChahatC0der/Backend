using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolERP.Domain.Fees.Entities;

namespace SchoolERP.Infrastructure.Persistence.Configurations.Fees;

public sealed class LateFeeLogConfiguration : IEntityTypeConfiguration<LateFeeLog>
{
    public void Configure(EntityTypeBuilder<LateFeeLog> builder)
    {
        builder.ToTable("LateFeeLogs");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.Property(x => x.InvoiceId).IsRequired();
        builder.Property(x => x.CalculatedDate).IsRequired();
        builder.Property(x => x.LateFeeAmount).HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.WaivedAmount).HasPrecision(18, 2);
        builder.Property(x => x.WaivedBy);
        builder.Property(x => x.WaiverReason).HasMaxLength(500);
        builder.Property(x => x.CreatedAt).IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property(x => x.UpdatedAt);

        // PERSISTED computed column — enforces one log per invoice per calendar day.
        builder.Property<DateTime>("CalculationDateOnly")
            .HasColumnName("CalculationDateOnly")
            .HasColumnType("date")
            .HasComputedColumnSql("CAST([CalculatedDate] AS DATE)", stored: true);

        builder.HasIndex("InvoiceId", "CalculationDateOnly")
            .HasDatabaseName("UQ_LateFeeLogs_Invoice_Daily")
            .IsUnique();

        builder.HasIndex(x => x.CalculatedDate)
            .HasDatabaseName("IX_LateFeeLogs_CalculatedDate");
    }
}
