using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolERP.Domain.Master.Entities;
using SchoolERP.Infrastructure.Persistence.Configurations.Common;

namespace SchoolERP.Infrastructure.Persistence.Configurations.Master;

public class MasterItemConfiguration : IEntityTypeConfiguration<MasterItem>
{
    public void Configure(EntityTypeBuilder<MasterItem> builder)
    {
        builder.ToTable("MasterItems");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.Property(x => x.CategoryId).IsRequired();
        builder.Property(x => x.Value).HasMaxLength(255).IsRequired();
        builder.Property(x => x.Code).HasMaxLength(50);
        builder.Property(x => x.Description).HasMaxLength(255);
        builder.Property(x => x.Metadata).HasColumnType("nvarchar(max)");
        builder.Property(x => x.SortOrder).HasDefaultValue(0);
        builder.Property(x => x.IsSystem).HasDefaultValue(false);
        builder.Property(x => x.IsActive).HasDefaultValue(true);
        builder.Property(x => x.IsDeleted).HasDefaultValue(false);
        builder.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        builder.Property(x => x.UpdatedAt).IsRequired(false);
        builder.Property(x => x.DeletedAt).IsRequired(false);

        // Relationship
        builder.HasOne(x => x.Category)
            .WithMany(c => c.Items)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        // Unique index (CategoryId, Value)
        builder.HasIndex(x => new { x.CategoryId, x.Value }).IsUnique();
        builder.HasIndex(x => new { x.CategoryId, x.SortOrder });
        builder.HasIndex(x => x.IsDeleted).HasFilter("[IsDeleted] = 0");
    }
}