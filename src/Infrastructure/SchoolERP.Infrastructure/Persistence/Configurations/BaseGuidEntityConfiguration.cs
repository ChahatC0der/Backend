using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolERP.Domain.Common;

namespace SchoolERP.Infrastructure.Persistence.Configurations;

/// <summary>
/// Base configuration for entities with GUID ID (uniqueidentifier)
/// e.g., Tenant, Branch, Student, Staff
/// 🔥 Id = Guid.NewGuid() C# se set hoga (constructor mein). SQL default nahi chahiye.
/// 🔥 CreatedAt, UpdatedAt, DeletedAt — C# set karega.
/// </summary>
public abstract class BaseGuidEntityConfiguration<TEntity> : IEntityTypeConfiguration<TEntity>
    where TEntity : GuidAuditableEntity
{
    public virtual void Configure(EntityTypeBuilder<TEntity> builder)
    {
        // 🔥 GUID ID — C# constructor mein set hota hai (Guid.NewGuid()), 
        // isliye ValueGeneratedNever() use kar rahe hain taaki EF core DB se expect na kare.
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();

        // 🔥 Soft Delete flag — default false
        builder.Property(e => e.IsDeleted).HasDefaultValue(false);

        // 🔥 Timestamps — SIRF C# SET KAREGA (SQL default nahi chahiye)
        builder.Property(e => e.CreatedAt).IsRequired();
        builder.Property(e => e.UpdatedAt).IsRequired(false);
        builder.Property(e => e.DeletedAt).IsRequired(false);
    }
}