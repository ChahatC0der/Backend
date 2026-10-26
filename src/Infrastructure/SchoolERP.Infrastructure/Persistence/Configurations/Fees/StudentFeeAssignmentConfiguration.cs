using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolERP.Domain.Fees.Entities;

namespace SchoolERP.Infrastructure.Persistence.Configurations.Fees;

public sealed class StudentFeeAssignmentConfiguration : IEntityTypeConfiguration<StudentFeeAssignment>
{
    public void Configure(EntityTypeBuilder<StudentFeeAssignment> builder)
    {
        builder.ToTable("StudentFeeAssignments");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.Property(x => x.StudentId).IsRequired();
        builder.Property(x => x.FeeStructureId).IsRequired();
        builder.Property(x => x.AcademicYearId).IsRequired();
        builder.Property(x => x.OverrideAmount).HasPrecision(18, 2);
        builder.Property(x => x.IsActive).HasDefaultValue(true);

        builder.Property(x => x.CreatedAt).IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property(x => x.UpdatedAt);

        builder.HasIndex(x => new { x.StudentId, x.AcademicYearId })
            .HasDatabaseName("UQ_StudentFeeAssignments_Active")
            .IsUnique()
            .HasFilter("[IsActive] = 1");
    }
}
