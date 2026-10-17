using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AdmissionEntity = SchoolERP.Domain.Student.Admission.Entities.Admission;
using SchoolERP.Infrastructure.Persistence.Configurations.Common;

namespace SchoolERP.Infrastructure.Persistence.Configurations.Student.Admission;

public class AdmissionConfiguration : BaseBranchOnlyEntityConfiguration<AdmissionEntity>
{
    public override void Configure(EntityTypeBuilder<AdmissionEntity> builder)
    {
        base.Configure(builder);

        builder.ToTable("Admissions");

        //builder.Property(x => x.TenantId).IsRequired();
        builder.Property(x => x.AdmissionNumber).HasMaxLength(50);
        builder.Property(x => x.RollNumber).HasMaxLength(20);
        builder.Property(x => x.PreviousSchool).HasMaxLength(255);
        builder.Property(x => x.FirstFeeAmount).HasColumnType("decimal(12,2)");
        builder.Property(x => x.FirstFeePaymentMode).HasMaxLength(20);
        builder.Property(x => x.Status).HasMaxLength(20).IsRequired().HasDefaultValue("in_progress");

        builder.HasIndex(x => new { x.BranchId, x.Status });
        builder.HasIndex(x => x.StudentId).HasFilter("[StudentId] IS NOT NULL");
        builder.HasIndex(x => new { x.ClassId, x.SectionId, x.AcademicYearId });
        builder.HasIndex(x => x.RollNumber).HasFilter("[RollNumber] IS NOT NULL");
        builder.HasIndex(x => x.CreatedAt);

        // One admission per registration
        builder.HasIndex(x => x.RegistrationId)
            .IsUnique()
            .HasFilter("[RegistrationId] IS NOT NULL AND [IsDeleted] = 0");

        // One admission per enquiry
        builder.HasIndex(x => x.EnquiryId)
            .IsUnique()
            .HasFilter("[EnquiryId] IS NOT NULL AND [IsDeleted] = 0");
    }
}
