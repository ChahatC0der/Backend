using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolERP.Domain.Academic.Entities;
using SchoolERP.Infrastructure.Persistence.Configurations.Common;

namespace SchoolERP.Infrastructure.Persistence.Configurations.Academic;

public class AcademicYearConfiguration : BaseBranchOnlyEntityConfiguration<AcademicYear>
{
    public override void Configure(EntityTypeBuilder<AcademicYear> builder)
    {
        base.Configure(builder); // sets TenantId + BranchId required, indexes, soft delete

        builder.ToTable("AcademicYears");

        builder.Property(x => x.Name).HasMaxLength(20).IsRequired();
        builder.Property(x => x.StartDate).IsRequired();
        builder.Property(x => x.EndDate).IsRequired();
        builder.Property(x => x.IsCurrent).HasDefaultValue(false);
        builder.Property(x => x.Status).HasMaxLength(20).HasDefaultValue("upcoming");

        builder.HasIndex(x => new { x.BranchId, x.Name }).IsUnique();
        builder.HasIndex(x => new { x.BranchId, x.IsCurrent }).IsUnique().HasFilter("[IsCurrent] = 1 AND [DeletedAt] IS NULL");
    }
}