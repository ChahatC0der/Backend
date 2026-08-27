using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolERP.Domain.Rbac.Entities;
using SchoolERP.Infrastructure.Persistence.Configurations.Common;

namespace SchoolERP.Infrastructure.Persistence.Configurations.Rbac;

public class ModuleConfiguration : BaseEntityConfiguration<Module>
{
    public override void Configure(EntityTypeBuilder<Module> builder)
    {
        base.Configure(builder);

        builder.ToTable("Modules");

        builder.Property(m => m.Name).HasMaxLength(100).IsRequired();
        builder.Property(m => m.Key).HasMaxLength(100).IsRequired();
        builder.Property(m => m.Description).HasMaxLength(255);
        builder.Property(m => m.SortOrder).HasDefaultValue(0);

        builder.HasOne(m => m.Parent)
            .WithMany(m => m.Children)
            .HasForeignKey(m => m.ParentId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(m => m.Key).IsUnique();
        builder.HasIndex(m => m.ParentId);
    }
}