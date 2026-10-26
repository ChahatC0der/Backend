using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolERP.Domain.Fees.Entities;
using SchoolERP.Domain.Fees.Enums;
using SchoolERP.Infrastructure.Persistence.Converters;

namespace SchoolERP.Infrastructure.Persistence.Configurations.Fees;

public sealed class StudentCreditLedgerConfiguration : IEntityTypeConfiguration<StudentCreditLedger>
{
    public void Configure(EntityTypeBuilder<StudentCreditLedger> builder)
    {
        builder.ToTable("StudentCreditLedger");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.Property(x => x.TenantId).IsRequired();
        builder.Property(x => x.StudentId).IsRequired();
        builder.Property(x => x.AcademicYearId);
        builder.Property(x => x.Amount).HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.BalanceAfter).HasPrecision(18, 2).IsRequired();

        builder.Property(x => x.TransactionType)
            .HasConversion(new EnumStringConverter<CreditTransactionType>(EnumMaps.CreditTransactionTypeMap))
            .HasMaxLength(30).IsRequired();

        builder.Property(x => x.ReferenceInvoiceId);
        builder.Property(x => x.ReferencePaymentId);
        builder.Property(x => x.Reason).HasMaxLength(500);
        builder.Property(x => x.CreatedAt).IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasIndex(x => new { x.StudentId, x.Id })
            .HasDatabaseName("IX_StudentCreditLedger_StudentId_Id")
            .IsDescending(false, true)
            .IncludeProperties(x => new { x.BalanceAfter, x.TransactionType });
    }
}
