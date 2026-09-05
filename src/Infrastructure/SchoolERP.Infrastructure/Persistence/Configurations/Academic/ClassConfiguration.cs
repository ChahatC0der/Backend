using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolERP.Domain.Academic.Entities;
using SchoolERP.Infrastructure.Persistence.Configurations.Common;

namespace SchoolERP.Infrastructure.Persistence.Configurations.Academic;

public class ClassConfiguration : BaseBranchOnlyEntityConfiguration<Class>
{
    public override void Configure(EntityTypeBuilder<Class> builder)
    {
        base.Configure(builder);

        builder.ToTable("Classes");

        builder.Property(x => x.ClassGroupId).IsRequired(false);
        builder.Property(x => x.Name).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Sequence).IsRequired();
        builder.Property(x => x.IsActive).HasDefaultValue(true);

        builder.HasOne(x => x.ClassGroup)
            .WithMany()
            .HasForeignKey(x => x.ClassGroupId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => new { x.BranchId, x.Sequence }).IsUnique();
    }
}