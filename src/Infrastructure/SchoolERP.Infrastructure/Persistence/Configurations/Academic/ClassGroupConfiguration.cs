using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolERP.Domain.Academic.Entities;
using SchoolERP.Infrastructure.Persistence.Configurations.Common;

namespace SchoolERP.Infrastructure.Persistence.Configurations.Academic;

public class ClassGroupConfiguration : BaseBranchOnlyEntityConfiguration<ClassGroup>
{
    public override void Configure(EntityTypeBuilder<ClassGroup> builder)
    {
        base.Configure(builder);

        builder.ToTable("ClassGroups");

        builder.Property(x => x.Name).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Sequence).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(255);
        builder.Property(x => x.IsActive).HasDefaultValue(true);

        builder.HasIndex(x => new { x.BranchId, x.Sequence }).IsUnique();
    }
}