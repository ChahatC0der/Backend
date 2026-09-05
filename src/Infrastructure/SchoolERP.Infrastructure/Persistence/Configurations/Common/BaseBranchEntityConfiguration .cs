using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolERP.Domain.Common;

namespace SchoolERP.Infrastructure.Persistence.Configurations.Common;

public abstract class BaseBranchEntityConfiguration<TEntity> : BaseTenantEntityConfiguration<TEntity>
    where TEntity : BaseTenantEntity, IMustHaveBranch
{
    public override void Configure(EntityTypeBuilder<TEntity> builder)
    {
        base.Configure(builder);   // sets TenantId required + index

        builder.Property(x => x.BranchId).IsRequired();
        builder.HasIndex(x => x.BranchId);
    }
}