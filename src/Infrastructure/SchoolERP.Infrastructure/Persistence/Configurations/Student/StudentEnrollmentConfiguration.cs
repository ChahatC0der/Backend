using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentEnrollmentEntity = SchoolERP.Domain.Student.Entities.StudentEnrollment;
using SchoolERP.Domain.Student.Entities;
using SchoolERP.Infrastructure.Persistence.Configurations.Common;

namespace SchoolERP.Infrastructure.Persistence.Configurations.Student;

public class StudentEnrollmentConfiguration : BaseEntityConfiguration<StudentEnrollmentEntity>
{
    public override void Configure(EntityTypeBuilder<StudentEnrollmentEntity> builder)
    {
        base.Configure(builder);

        builder.ToTable("StudentEnrollments");

        builder.Property(x => x.RollNumber).HasMaxLength(20);
        builder.Property(x => x.IsCurrent).HasDefaultValue(true);
        builder.Property(x => x.EnrolledAt).HasDefaultValueSql("CAST(GETUTCDATE() AS DATE)");

        builder.HasOne(x => x.Student)
            .WithMany(s => s.Enrollments)
            .HasForeignKey(x => x.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.StudentId, x.AcademicYearId }).IsUnique();
        builder.HasIndex(x => new { x.ClassId, x.SectionId, x.AcademicYearId });
        builder.HasIndex(x => new { x.StudentId, x.IsCurrent }).HasFilter("[IsCurrent] = 1");
    }
}
