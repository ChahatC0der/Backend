using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolERP.Domain.Student.Entities;
using SchoolERP.Infrastructure.Persistence.Configurations.Common;
using StaffEntity = SchoolERP.Domain.Staff.Entities.Staff;

namespace SchoolERP.Infrastructure.Persistence.Configurations.Student;

public class ParentConfiguration : BaseEntityConfiguration<Parent>
{
    public override void Configure(EntityTypeBuilder<Parent> builder)
    {
        base.Configure(builder);

        builder.ToTable("Parents");

        builder.Property(x => x.TenantId).IsRequired();
        builder.Property(x => x.StaffId).IsRequired(false);
        builder.Property(x => x.ParentType).HasMaxLength(20).IsRequired();
        builder.Property(x => x.FirstName).HasMaxLength(255).IsRequired();
        builder.Property(x => x.LastName).HasMaxLength(255);
        builder.Property(x => x.Email).HasMaxLength(255);
        builder.Property(x => x.Mobile).HasMaxLength(20);
        builder.Property(x => x.Occupation).HasMaxLength(255);
        builder.Property(x => x.IsPrimary).HasDefaultValue(false);

        builder.HasOne(x => x.Student)
            .WithMany(s => s.Parents)
            .HasForeignKey(x => x.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        // 🔥 Staff relationship
        builder.HasOne(x => x.Staff)
            .WithMany()
            .HasForeignKey(x => x.StaffId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => x.StudentId);
        builder.HasIndex(x => x.Mobile);
        builder.HasIndex(x => x.Email);
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.StaffId).HasFilter("[StaffId] IS NOT NULL");
    }
}
