using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AluminiEntity = SchoolERP.Domain.Student.Alumni.Entities.Alumni;
using SchoolERP.Infrastructure.Persistence.Configurations.Common;

namespace SchoolERP.Infrastructure.Persistence.Configurations.Student.Alumni;

public class AlumniConfiguration : BaseBranchOnlyEntityConfiguration<AluminiEntity>
{
    public override void Configure(EntityTypeBuilder<AluminiEntity> builder)
    {
        base.Configure(builder);
        builder.ToTable("Alumni");

        builder.Property(x => x.TenantId).IsRequired();
        builder.Property(x => x.AlumniId).HasMaxLength(50).IsRequired();
        builder.HasIndex(x => x.AlumniId).IsUnique();

        builder.Property(x => x.FirstName).HasMaxLength(255).IsRequired();
        builder.Property(x => x.LastName).HasMaxLength(255);
        builder.Property(x => x.Gender).HasMaxLength(20);

        builder.Property(x => x.CurrentStatus).HasMaxLength(20).IsRequired().HasDefaultValue("not_employed");
        builder.Property(x => x.FinalPercentage).HasColumnType("decimal(5,2)");

        builder.Property(x => x.Occupation).HasMaxLength(255);
        builder.Property(x => x.CompanyName).HasMaxLength(255);
        builder.Property(x => x.Industry).HasMaxLength(100);
        builder.Property(x => x.HigherEducationInstitution).HasMaxLength(255);
        builder.Property(x => x.HigherEducationCourse).HasMaxLength(255);
        builder.Property(x => x.ContactEmail).HasMaxLength(255);
        builder.Property(x => x.ContactMobile).HasMaxLength(20);
        builder.Property(x => x.Address).HasColumnType("nvarchar(max)");
        builder.Property(x => x.SocialLinks).HasColumnType("nvarchar(max)");
        builder.Property(x => x.PhotoUrl).HasMaxLength(500);
        builder.Property(x => x.PrivacySettings).HasColumnType("nvarchar(max)").HasDefaultValue("{}");
        builder.Property(x => x.CommunicationPreferences).HasColumnType("nvarchar(max)").HasDefaultValue("{}");
        builder.Property(x => x.Tags).HasColumnType("nvarchar(max)");

        builder.HasIndex(x => new { x.BranchId, x.LastClassId });
        builder.HasIndex(x => x.PassingAcademicYearId);
        builder.HasIndex(x => x.CurrentStatus);
        builder.HasIndex(x => x.StudentId).HasFilter("[StudentId] IS NOT NULL");
        builder.HasIndex(x => x.ContactMobile);
    }
}
