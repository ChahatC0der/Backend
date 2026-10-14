using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolERP.Domain.Student.Admission.Entities;
using SchoolERP.Infrastructure.Persistence.Configurations.Common;

namespace SchoolERP.Infrastructure.Persistence.Configurations.Student.Admission;

public class AdmissionAuditLogConfiguration : BaseEntityConfiguration<AdmissionAuditLog>
{
    public override void Configure(EntityTypeBuilder<AdmissionAuditLog> builder)
    {
        base.Configure(builder);

        builder.ToTable("AdmissionAuditLogs");

        builder.Property(x => x.OldStatus).HasMaxLength(20);
        builder.Property(x => x.NewStatus).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Reason).HasColumnType("nvarchar(max)");

        builder.HasOne(x => x.Admission)
            .WithMany(a => a.AuditLogs)
            .HasForeignKey(x => x.AdmissionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.AdmissionId, x.CreatedAt });
        builder.HasIndex(x => x.PerformedBy);

        // Matching filter to avoid required-end warning
        builder.HasQueryFilter(x => !x.Admission.IsDeleted);
    }
}
