using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolERP.Domain.Common;

namespace SchoolERP.Infrastructure.Persistence.Configurations.Common;

public abstract class BaseBranchOnlyEntityConfiguration<TEntity> : BaseEntityConfiguration<TEntity>
    where TEntity : BaseEntity, IMustHaveBranch
{
    public override void Configure(EntityTypeBuilder<TEntity> builder)
    {
        base.Configure(builder);

        builder.Property(x => x.BranchId).IsRequired();
        builder.HasIndex(x => x.BranchId);
    }
}