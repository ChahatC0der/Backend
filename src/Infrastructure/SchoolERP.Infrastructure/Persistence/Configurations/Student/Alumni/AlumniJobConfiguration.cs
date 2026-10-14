using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolERP.Domain.Student.Alumni.Entities;
using SchoolERP.Infrastructure.Persistence.Configurations.Common;

namespace SchoolERP.Infrastructure.Persistence.Configurations.Student.Alumni;

public class AlumniJobConfiguration : BaseBranchOnlyEntityConfiguration<AlumniJob>
{
    public override void Configure(EntityTypeBuilder<AlumniJob> builder)
    {
        base.Configure(builder);
        builder.ToTable("AlumniJobs");

        builder.Property(x => x.Title).HasMaxLength(255).IsRequired();
        builder.Property(x => x.Company).HasMaxLength(255).IsRequired();
        builder.Property(x => x.Description).HasColumnType("nvarchar(max)");
        builder.Property(x => x.Location).HasMaxLength(255);
        builder.Property(x => x.ApplicationLink).HasMaxLength(500);

        builder.HasIndex(x => x.BranchId);
        builder.HasIndex(x => x.ExpiryDate);
    }
}
