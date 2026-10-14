using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolERP.Domain.Student.Alumni.Entities;
using SchoolERP.Infrastructure.Persistence.Configurations.Common;

namespace SchoolERP.Infrastructure.Persistence.Configurations.Student.Alumni;

public class AlumniSuccessStoryConfiguration : BaseEntityConfiguration<AlumniSuccessStory>
{
    public override void Configure(EntityTypeBuilder<AlumniSuccessStory> builder)
    {
        base.Configure(builder);
        builder.ToTable("AlumniSuccessStories");

        builder.Property(x => x.Story).HasColumnType("nvarchar(max)").IsRequired();
        builder.Property(x => x.PhotoUrl).HasMaxLength(500);
        builder.Property(x => x.VideoUrl).HasMaxLength(500);

        builder.HasOne(x => x.Alumni)
            .WithMany(a => a.SuccessStories)
            .HasForeignKey(x => x.AlumniId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.AlumniId);
        builder.HasIndex(x => x.IsPublished).HasFilter("[IsPublished] = 1");

        builder.HasQueryFilter(x => !x.Alumni.IsDeleted);
    }
}
