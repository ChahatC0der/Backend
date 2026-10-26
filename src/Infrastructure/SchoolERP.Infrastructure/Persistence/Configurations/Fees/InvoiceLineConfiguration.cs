using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolERP.Domain.Fees.Entities;

namespace SchoolERP.Infrastructure.Persistence.Configurations.Fees;

public sealed class InvoiceLineConfiguration : IEntityTypeConfiguration<InvoiceLine>
{
    public void Configure(EntityTypeBuilder<InvoiceLine> builder)
    {
        builder.ToTable("InvoiceLines");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.Property(x => x.InvoiceId).IsRequired();
        builder.Property(x => x.FeeHeadId).IsRequired();
        builder.Property(x => x.Amount).HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.ConcessionApplied).HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.NetAmount).HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.FeeHeadNameSnapshot).HasMaxLength(200).IsRequired();

        builder.Property(x => x.IsLateFeeLine).HasDefaultValue(false).IsRequired();
        builder.Property(x => x.SourceInvoiceLineId);

        builder.Property(x => x.LateFeeApplicable).HasDefaultValue(false);
        builder.Property(x => x.LateFeeTypeSnapshot).HasMaxLength(30);
        builder.Property(x => x.LateFeeValueSnapshot).HasPrecision(18, 2);
        builder.Property(x => x.LateFeeGraceDaysSnapshot);
        builder.Property(x => x.LateFeeCapPercentSnapshot).HasPrecision(5, 2);
        builder.Property(x => x.LateFeeSlabConfigSnapshot).HasColumnType("nvarchar(max)");
        builder.Property(x => x.LateFeeCapBase).HasPrecision(18, 2).IsRequired();

        builder.Property(x => x.CreatedAt).IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasIndex(x => x.InvoiceId).HasDatabaseName("IX_InvoiceLines_InvoiceId");
        builder.HasIndex(x => new { x.InvoiceId, x.IsLateFeeLine, x.SourceInvoiceLineId })
            .HasDatabaseName("IX_InvoiceLines_Invoice_LateFee");

        builder.HasOne<InvoiceLine>()
            .WithMany()
            .HasForeignKey(x => x.SourceInvoiceLineId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);
    }
}
