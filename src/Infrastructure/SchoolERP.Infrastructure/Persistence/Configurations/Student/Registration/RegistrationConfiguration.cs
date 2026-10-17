using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegistrationEntity = SchoolERP.Domain.Student.Registration.Entities.Registration;
using SchoolERP.Infrastructure.Persistence.Configurations.Common;

namespace SchoolERP.Infrastructure.Persistence.Configurations.Student.Registration;

public class RegistrationConfiguration : BaseBranchOnlyEntityConfiguration<RegistrationEntity>
{
    public override void Configure(EntityTypeBuilder<RegistrationEntity> builder)
    {
        base.Configure(builder);

        builder.ToTable("Registrations");

        //builder.Property(x => x.TenantId).IsRequired();
        builder.Property(x => x.RegistrationNumber).HasMaxLength(50).IsRequired();
        builder.HasIndex(x => x.RegistrationNumber).IsUnique();

        builder.Property(x => x.StudentFirstName).HasMaxLength(255).IsRequired();
        builder.Property(x => x.StudentLastName).HasMaxLength(255);
        builder.Property(x => x.Gender).HasMaxLength(20);
        builder.Property(x => x.Birthplace).HasMaxLength(100);
        builder.Property(x => x.Nationality).HasMaxLength(100);
        builder.Property(x => x.MotherTongue).HasMaxLength(50);
        builder.Property(x => x.Religion).HasMaxLength(50);
        builder.Property(x => x.AadharNumber).HasMaxLength(20);

        builder.Property(x => x.FatherName).HasMaxLength(255);
        builder.Property(x => x.FatherMobile).HasMaxLength(20);
        builder.Property(x => x.FatherEmail).HasMaxLength(255);
        builder.Property(x => x.FatherOccupation).HasMaxLength(255);
        builder.Property(x => x.MotherName).HasMaxLength(255);
        builder.Property(x => x.MotherMobile).HasMaxLength(20);
        builder.Property(x => x.MotherEmail).HasMaxLength(255);
        builder.Property(x => x.MotherOccupation).HasMaxLength(255);
        builder.Property(x => x.GuardianName).HasMaxLength(255);
        builder.Property(x => x.GuardianMobile).HasMaxLength(20);
        builder.Property(x => x.GuardianEmail).HasMaxLength(255);
        builder.Property(x => x.GuardianOccupation).HasMaxLength(255);

        builder.Property(x => x.ParentPrimaryMobile).HasMaxLength(20).IsRequired();
        builder.Property(x => x.ParentPrimaryEmail).HasMaxLength(255);

        builder.Property(x => x.PresentAddress).HasColumnType("nvarchar(max)");
        builder.Property(x => x.PermanentAddress).HasColumnType("nvarchar(max)");

        builder.Property(x => x.PreviousSchool).HasMaxLength(255);
        builder.Property(x => x.PreviousBoard).HasMaxLength(100);
        builder.Property(x => x.PreviousPercentage).HasColumnType("decimal(5,2)");
        builder.Property(x => x.TransferCertificateUrl).HasMaxLength(500);

        builder.Property(x => x.Source).HasMaxLength(50);
        builder.Property(x => x.MedicalConditions).HasColumnType("nvarchar(max)");
        builder.Property(x => x.CustomFields).HasColumnType("nvarchar(max)");

        builder.Property(x => x.RegistrationFeeAmount).HasColumnType("decimal(12,2)");
        builder.Property(x => x.RegistrationFeePaymentMode).HasMaxLength(20);

        builder.Property(x => x.InterviewMode).HasMaxLength(20);
        builder.Property(x => x.InterviewResult).HasMaxLength(20);
        builder.Property(x => x.InterviewFeedback).HasColumnType("nvarchar(max)");

        builder.Property(x => x.RejectionReason).HasColumnType("nvarchar(max)");

        builder.Property(x => x.Status).HasMaxLength(20).IsRequired().HasDefaultValue("draft");

        // Indexes
        builder.HasIndex(x => new { x.BranchId, x.Status });
        builder.HasIndex(x => x.ParentPrimaryMobile);
        builder.HasIndex(x => new { x.ClassId, x.AcademicYearId });
        builder.HasIndex(x => x.EnquiryId).HasFilter("[EnquiryId] IS NOT NULL");
        builder.HasIndex(x => x.ApprovedBy).HasFilter("[ApprovedBy] IS NOT NULL");
        builder.HasIndex(x => x.ConvertedToStudentId).HasFilter("[ConvertedToStudentId] IS NOT NULL");
    }
}
