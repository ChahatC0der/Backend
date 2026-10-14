using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolERP.Domain.Student.Entities;
using SchoolERP.Infrastructure.Persistence.Configurations.Common;

namespace SchoolERP.Infrastructure.Persistence.Configurations.Student;

public class FamilyConfiguration : BaseBranchOnlyEntityConfiguration<Family>
{
    public override void Configure(EntityTypeBuilder<Family> builder)
    {
        base.Configure(builder);

        builder.ToTable("Families");

        builder.Property(x => x.TenantId).IsRequired();
        builder.Property(x => x.HeadName).HasMaxLength(255);
        builder.Property(x => x.HeadMobile).HasMaxLength(20);
        builder.Property(x => x.HeadEmail).HasMaxLength(255);

        builder.HasIndex(x => new { x.TenantId, x.BranchId });
        builder.HasIndex(x => x.HeadMobile);
    }
}
