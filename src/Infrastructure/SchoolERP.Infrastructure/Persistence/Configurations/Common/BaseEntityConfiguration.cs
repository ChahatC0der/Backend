using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolERP.Domain.Common;

namespace SchoolERP.Infrastructure.Persistence.Configurations.Common;

/// <summary>
/// Base configuration for all entities that inherit from BaseEntity.
/// Applies common settings: IsDeleted, CreatedAt, UpdatedAt, DeletedAt.
/// </summary>
public abstract class BaseEntityConfiguration<TEntity> : IEntityTypeConfiguration<TEntity>
    where TEntity : BaseEntity
{
    public virtual void Configure(EntityTypeBuilder<TEntity> builder)
    {
        // 🔥 Common base entity configuration
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        // Soft delete filter (handled in global query filter)
        builder.Property(e => e.IsDeleted).HasDefaultValue(false);
        builder.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        builder.Property(e => e.UpdatedAt).IsRequired(false);
        builder.Property(e => e.DeletedAt).IsRequired(false);
    }
}