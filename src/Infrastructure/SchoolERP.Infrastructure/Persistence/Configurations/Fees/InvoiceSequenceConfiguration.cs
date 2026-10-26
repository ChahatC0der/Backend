using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolERP.Domain.Fees.Entities;

namespace SchoolERP.Infrastructure.Persistence.Configurations.Fees;

public sealed class InvoiceSequenceConfiguration : IEntityTypeConfiguration<InvoiceSequence>
{
    public void Configure(EntityTypeBuilder<InvoiceSequence> builder)
    {
        builder.ToTable("InvoiceSequences");
        builder.HasKey(x => new { x.BranchId, x.AcademicYearCode, x.SequenceType });

        builder.Property(x => x.AcademicYearCode).HasMaxLength(10).IsRequired();
        builder.Property(x => x.SequenceType).HasMaxLength(20).IsRequired();
        builder.Property(x => x.CurrentSequence).IsRequired();
    }
}
