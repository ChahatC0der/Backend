using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolERP.Domain.Student.Alumni.Entities;
using SchoolERP.Infrastructure.Persistence.Configurations.Common;

namespace SchoolERP.Infrastructure.Persistence.Configurations.Student.Alumni;

public class AlumniEventConfiguration : BaseBranchOnlyEntityConfiguration<AlumniEvent>
{
    public override void Configure(EntityTypeBuilder<AlumniEvent> builder)
    {
        base.Configure(builder);
        builder.ToTable("AlumniEvents");

        builder.Property(x => x.Name).HasMaxLength(255).IsRequired();
        builder.Property(x => x.Description).HasColumnType("nvarchar(max)");
        builder.Property(x => x.Venue).HasMaxLength(255);
        builder.Property(x => x.EventType).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Fee).HasColumnType("decimal(12,2)");
        builder.Property(x => x.IsRegistrationRequired).HasDefaultValue(true);

        builder.HasIndex(x => new { x.BranchId, x.EventDate });
        builder.HasIndex(x => x.EventType);
    }
}
