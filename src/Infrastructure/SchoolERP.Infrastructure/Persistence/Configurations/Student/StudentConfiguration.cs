using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentEntity = SchoolERP.Domain.Student.Entities.Student;
using SchoolERP.Domain.Student.Entities;
using SchoolERP.Infrastructure.Persistence.Configurations.Common;

namespace SchoolERP.Infrastructure.Persistence.Configurations.Student;

public class StudentConfiguration : BaseBranchOnlyEntityConfiguration<StudentEntity>
{
    public override void Configure(EntityTypeBuilder<StudentEntity> builder)
    {
        base.Configure(builder);

        builder.ToTable("Students");

        builder.Property(x => x.TenantId).IsRequired();
        builder.Property(x => x.UserId).IsRequired(false);

        builder.Property(x => x.EnrollmentId).HasMaxLength(50).IsRequired();
        builder.HasIndex(x => x.EnrollmentId).IsUnique();

        builder.Property(x => x.FirstName).HasMaxLength(255).IsRequired();
        builder.Property(x => x.LastName).HasMaxLength(255);
        builder.Property(x => x.Gender).HasMaxLength(20);
        builder.Property(x => x.BloodGroup).HasMaxLength(10);
        builder.Property(x => x.Nationality).HasMaxLength(100);
        builder.Property(x => x.Religion).HasMaxLength(50);
        builder.Property(x => x.MotherTongue).HasMaxLength(50);

        builder.Property(x => x.PersonalEmail).HasMaxLength(255);
        builder.Property(x => x.PersonalMobile).HasMaxLength(20);
        builder.Property(x => x.AlternateMobile).HasMaxLength(20);
        builder.Property(x => x.Address).HasColumnType("nvarchar(max)");

        builder.Property(x => x.RollNumber).HasMaxLength(20);

        builder.Property(x => x.Status).HasMaxLength(20).IsRequired().HasDefaultValue("enrolled");
        builder.Property(x => x.PhotoUrl).HasMaxLength(500);
        builder.Property(x => x.CustomFields).HasColumnType("nvarchar(max)");

        builder.HasIndex(x => new { x.BranchId, x.Status });
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.PersonalMobile);
        builder.HasIndex(x => new { x.FirstName, x.LastName });
        builder.HasIndex(x => x.Status);
    }
}
