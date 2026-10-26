using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolERP.Domain.Fees.Entities;
using SchoolERP.Domain.Fees.Enums;
using SchoolERP.Infrastructure.Persistence.Converters;

namespace SchoolERP.Infrastructure.Persistence.Configurations.Fees;

public sealed class ConcessionAllocationConfiguration : IEntityTypeConfiguration<ConcessionAllocation>
{
    public void Configure(EntityTypeBuilder<ConcessionAllocation> builder)
    {
        builder.ToTable("ConcessionAllocations");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.Property(x => x.ConcessionId).IsRequired();
        builder.Property(x => x.StudentId).IsRequired();
        builder.Property(x => x.AcademicYearId).IsRequired();

        builder.Property(x => x.Status)
            .HasConversion(new EnumStringConverter<ConcessionAllocationStatus>(EnumMaps.ConcessionAllocationStatusMap))
            .HasMaxLength(20).IsRequired();

        builder.Property(x => x.ApprovedBy);
        builder.Property(x => x.ApprovedAt);
        builder.Property(x => x.RejectionReason).HasMaxLength(500);
        builder.Property(x => x.CreatedAt).IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property(x => x.UpdatedAt);

        builder.HasIndex(x => new { x.ConcessionId, x.StudentId, x.AcademicYearId })
            .HasDatabaseName("UQ_ConcessionAllocations_ActivePerStudent")
            .IsUnique()
            .HasFilter("[Status] IN ('pending', 'approved')");

        builder.HasIndex(x => new { x.StudentId, x.AcademicYearId, x.Status })
            .HasDatabaseName("IX_ConcessionAllocations_Student_AY_Status");
    }
}
