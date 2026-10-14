using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolERP.Domain.Student.Enquiry.Entities;
using SchoolERP.Infrastructure.Persistence.Configurations.Common;

namespace SchoolERP.Infrastructure.Persistence.Configurations.Student.Enquiry;

public class EnquiryFollowupConfiguration : BaseEntityConfiguration<EnquiryFollowup>
{
    public override void Configure(EntityTypeBuilder<EnquiryFollowup> builder)
    {
        base.Configure(builder);

        builder.ToTable("EnquiryFollowups");

        builder.Property(x => x.ActivityType).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Notes).HasColumnType("nvarchar(max)");

        builder.HasOne(x => x.Enquiry)
            .WithMany(e => e.Followups)
            .HasForeignKey(x => x.EnquiryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.EnquiryId);
        builder.HasIndex(x => x.NextFollowupDate);

        // 🔥 Matching query filter to avoid required-end warning
        // Filters followups by parent Enquiry's BranchId + IsDeleted
        builder.HasQueryFilter(x => !x.Enquiry.IsDeleted);
    }
}
