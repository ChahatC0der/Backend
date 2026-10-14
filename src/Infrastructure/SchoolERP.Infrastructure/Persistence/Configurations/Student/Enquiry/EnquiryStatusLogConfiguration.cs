using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolERP.Domain.Student.Enquiry.Entities;
using SchoolERP.Infrastructure.Persistence.Configurations.Common;

namespace SchoolERP.Infrastructure.Persistence.Configurations.Student.Enquiry;

public class EnquiryStatusLogConfiguration : BaseEntityConfiguration<EnquiryStatusLog>
{
    public override void Configure(EntityTypeBuilder<EnquiryStatusLog> builder)
    {
        base.Configure(builder);

        builder.ToTable("EnquiryStatusLogs");

        builder.Property(x => x.OldStatus).HasMaxLength(20);
        builder.Property(x => x.NewStatus).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Reason).HasColumnType("nvarchar(max)");

        builder.HasOne(x => x.Enquiry)
            .WithMany(e => e.StatusLogs)
            .HasForeignKey(x => x.EnquiryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.EnquiryId, x.CreatedAt });
        builder.HasIndex(x => x.PerformedBy);

        // 🔥 Matching query filter to avoid required-end warning
        builder.HasQueryFilter(x => !x.Enquiry.IsDeleted);
    }
}
