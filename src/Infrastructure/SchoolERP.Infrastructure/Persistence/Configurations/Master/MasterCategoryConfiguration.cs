using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolERP.Domain.Master.Entities;
using SchoolERP.Infrastructure.Persistence.Configurations.Common;

namespace SchoolERP.Infrastructure.Persistence.Configurations.Master;

public class MasterCategoryConfiguration : IEntityTypeConfiguration<MasterCategory>
{
    public void Configure(EntityTypeBuilder<MasterCategory> builder)
    {
        builder.ToTable("MasterCategories");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.Property(x => x.ModuleId).IsRequired();
        builder.Property(x => x.TenantId).IsRequired(false);     // nullable
        builder.Property(x => x.Key).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(255);
        builder.Property(x => x.IsSystem).HasDefaultValue(false);
        builder.Property(x => x.IsActive).HasDefaultValue(true);
        builder.Property(x => x.IsDeleted).HasDefaultValue(false);
        builder.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        builder.Property(x => x.UpdatedAt).IsRequired(false);
        builder.Property(x => x.DeletedAt).IsRequired(false);

        // Relationships
        builder.HasOne(x => x.Module)
            .WithMany()
            .HasForeignKey(x => x.ModuleId)
            .OnDelete(DeleteBehavior.Cascade);

        // Unique index (ModuleId, TenantId, Key)
        builder.HasIndex(x => new { x.ModuleId, x.TenantId, x.Key }).IsUnique();

        builder.HasIndex(x => x.TenantId);
        builder.HasIndex(x => x.IsDeleted).HasFilter("[IsDeleted] = 0");
    }
}