using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StaffEntity = SchoolERP.Domain.Staff.Entities.Staff;
using SchoolERP.Infrastructure.Persistence.Configurations.Common;

namespace SchoolERP.Infrastructure.Persistence.Configurations.Staff;

public class StaffConfiguration : BaseBranchOnlyEntityConfiguration<StaffEntity>
{
    public override void Configure(EntityTypeBuilder<StaffEntity> builder)
    {
        base.Configure(builder);

        builder.ToTable("Staff");

        builder.Property(x => x.StaffId).HasMaxLength(50).IsRequired();
        builder.HasIndex(x => x.StaffId).IsUnique();

        builder.Property(x => x.FirstName).HasMaxLength(255).IsRequired();
        builder.Property(x => x.LastName).HasMaxLength(255);
        builder.Property(x => x.Gender).HasMaxLength(20);
        builder.Property(x => x.BloodGroup).HasMaxLength(10);
        builder.Property(x => x.MaritalStatus).HasMaxLength(20);
        builder.Property(x => x.Nationality).HasMaxLength(100);

        builder.Property(x => x.PersonalEmail).HasMaxLength(255);
        builder.Property(x => x.WorkEmail).HasMaxLength(255);
        builder.Property(x => x.PersonalMobile).HasMaxLength(20);
        builder.Property(x => x.WorkMobile).HasMaxLength(20);
        builder.Property(x => x.EmergencyContactName).HasMaxLength(255);
        builder.Property(x => x.EmergencyContactPhone).HasMaxLength(20);

        builder.Property(x => x.EmploymentType).HasMaxLength(20).IsRequired();
        builder.Property(x => x.StaffType).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Designation).HasMaxLength(100).IsRequired();

        builder.Property(x => x.Qualification).HasMaxLength(255);
        builder.Property(x => x.Specialization).HasMaxLength(255);
        builder.Property(x => x.ExperienceYears).HasColumnType("decimal(3,1)");
        builder.Property(x => x.PreviousInstitution).HasMaxLength(255);

        builder.Property(x => x.PhotoUrl).HasMaxLength(500);
        builder.Property(x => x.Status).HasMaxLength(20).IsRequired().HasDefaultValue("active");
        builder.Property(x => x.CustomFields).HasColumnType("nvarchar(max)");

        builder.Property(x => x.TenantId).IsRequired();

        // Relationships
        builder.HasOne(x => x.Department)
            .WithMany(d => d.StaffMembers)
            .HasForeignKey(x => x.DepartmentId)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes
        builder.HasIndex(x => new { x.BranchId, x.Status });
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.DepartmentId);
        builder.HasIndex(x => x.PersonalMobile);
        builder.HasIndex(x => x.Designation);
        builder.HasIndex(x => x.StaffType);
    }
}