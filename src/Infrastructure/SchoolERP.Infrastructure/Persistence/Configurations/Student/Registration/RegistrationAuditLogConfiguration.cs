using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolERP.Domain.Student.Registration.Entities;
using SchoolERP.Infrastructure.Persistence.Configurations.Common;

namespace SchoolERP.Infrastructure.Persistence.Configurations.Student.Registration;

public class RegistrationAuditLogConfiguration : BaseEntityConfiguration<RegistrationAuditLog>
{
    public override void Configure(EntityTypeBuilder<RegistrationAuditLog> builder)
    {
        base.Configure(builder);

        builder.ToTable("RegistrationAuditLogs");

        builder.Property(x => x.OldStatus).HasMaxLength(20);
        builder.Property(x => x.NewStatus).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Reason).HasColumnType("nvarchar(max)");

        builder.HasOne(x => x.Registration)
            .WithMany(r => r.AuditLogs)
            .HasForeignKey(x => x.RegistrationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.RegistrationId, x.CreatedAt });
        builder.HasIndex(x => x.PerformedBy);

        // Matching filter
        builder.HasQueryFilter(x => !x.Registration.IsDeleted);
    }
}
