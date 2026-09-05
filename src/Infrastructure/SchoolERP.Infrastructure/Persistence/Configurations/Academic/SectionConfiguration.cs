using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolERP.Domain.Academic.Entities;
using SchoolERP.Infrastructure.Persistence.Configurations.Common;

namespace SchoolERP.Infrastructure.Persistence.Configurations.Academic;

public class SectionConfiguration : BaseBranchOnlyEntityConfiguration<Section>
{
    public override void Configure(EntityTypeBuilder<Section> builder)
    {
        base.Configure(builder);

        builder.ToTable("Sections");

        builder.Property(x => x.ClassId).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(10).IsRequired();
        builder.Property(x => x.Capacity).IsRequired(false);

        builder.HasOne(x => x.Class)
            .WithMany(c => c.Sections)
            .HasForeignKey(x => x.ClassId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.ClassId, x.Name }).IsUnique();
    }
}