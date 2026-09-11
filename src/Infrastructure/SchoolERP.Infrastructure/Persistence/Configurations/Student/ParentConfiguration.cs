using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ParentEntity = SchoolERP.Domain.Student.Entities.Parent;
using SchoolERP.Domain.Student.Entities;
using SchoolERP.Infrastructure.Persistence.Configurations.Common;

namespace SchoolERP.Infrastructure.Persistence.Configurations.Student;

public class ParentConfiguration : BaseEntityConfiguration<ParentEntity>
{
    public override void Configure(EntityTypeBuilder<ParentEntity> builder)
    {
        base.Configure(builder);

        builder.ToTable("Parents");

        builder.Property(x => x.TenantId).IsRequired();
        builder.Property(x => x.ParentType).HasMaxLength(20).IsRequired();
        builder.Property(x => x.FirstName).HasMaxLength(255).IsRequired();
        builder.Property(x => x.LastName).HasMaxLength(255);
        builder.Property(x => x.Email).HasMaxLength(255);
        builder.Property(x => x.Mobile).HasMaxLength(20);
        builder.Property(x => x.Occupation).HasMaxLength(255);
        builder.Property(x => x.IsPrimary).HasDefaultValue(false);

        builder.HasOne(x => x.Student)
            .WithMany(s => s.Parents)
            .HasForeignKey(x => x.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.StudentId);
        builder.HasIndex(x => x.Mobile);
        builder.HasIndex(x => x.Email);
        builder.HasIndex(x => x.UserId);
    }
}
