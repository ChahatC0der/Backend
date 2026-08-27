using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolERP.Domain.Common; // IMustHaveTenant, BaseEntity (if applicable)

namespace SchoolERP.Infrastructure.Persistence.Configurations.Common;

public abstract class BaseTenantEntityConfiguration<TEntity> : BaseEntityConfiguration<TEntity>
    where TEntity : BaseEntity, IMustHaveTenant
{
    public override void Configure(EntityTypeBuilder<TEntity> builder)
    {
        base.Configure(builder);

        builder.Property(x => x.TenantId)
            .IsRequired();

        builder.HasIndex(x => x.TenantId);
    }
}