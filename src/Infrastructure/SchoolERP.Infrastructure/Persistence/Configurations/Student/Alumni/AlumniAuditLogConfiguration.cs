using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolERP.Domain.Student.Alumni.Entities;
using SchoolERP.Infrastructure.Persistence.Configurations.Common;

namespace SchoolERP.Infrastructure.Persistence.Configurations.Student.Alumni;

public class AlumniAuditLogConfiguration : BaseEntityConfiguration<AlumniAuditLog>
{
    public override void Configure(EntityTypeBuilder<AlumniAuditLog> builder)
    {
        base.Configure(builder);
        builder.ToTable("AlumniAuditLogs");

        builder.Property(x => x.TenantId).IsRequired();
        builder.Property(x => x.Action).HasMaxLength(50).IsRequired();
        builder.Property(x => x.OldValues).HasColumnType("nvarchar(max)");
        builder.Property(x => x.NewValues).HasColumnType("nvarchar(max)");

        builder.HasOne(x => x.Alumni)
            .WithMany()
            .HasForeignKey(x => x.AlumniId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.AlumniId, x.CreatedAt });
        builder.HasIndex(x => x.TenantId);
        builder.HasIndex(x => x.PerformedBy);

        builder.HasQueryFilter(x => !x.Alumni.IsDeleted);
    }
}
