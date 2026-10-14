using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EnquiryEntity = SchoolERP.Domain.Student.Enquiry.Entities.Enquiry;
using SchoolERP.Infrastructure.Persistence.Configurations.Common;

namespace SchoolERP.Infrastructure.Persistence.Configurations.Student.Enquiry;

public class EnquiryConfiguration : BaseBranchOnlyEntityConfiguration<EnquiryEntity>
{
    public override void Configure(EntityTypeBuilder<EnquiryEntity> builder)
    {
        base.Configure(builder);

        builder.ToTable("Enquiries");

        builder.Property(x => x.Source).HasMaxLength(50).IsRequired();
        builder.Property(x => x.StudentFirstName).HasMaxLength(255);
        builder.Property(x => x.StudentLastName).HasMaxLength(255);
        builder.Property(x => x.ParentName).HasMaxLength(255).IsRequired();
        builder.Property(x => x.Mobile).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Email).HasMaxLength(255);
        builder.Property(x => x.Message).HasColumnType("nvarchar(max)");
        builder.Property(x => x.Status).HasMaxLength(20).IsRequired().HasDefaultValue("new");

        builder.HasIndex(x => new { x.BranchId, x.Status });
        builder.HasIndex(x => x.Mobile);
        builder.HasIndex(x => x.AssignedTo);
        builder.HasIndex(x => x.ClassId);
        builder.HasIndex(x => x.ConvertedToRegistrationId);
    }
}
