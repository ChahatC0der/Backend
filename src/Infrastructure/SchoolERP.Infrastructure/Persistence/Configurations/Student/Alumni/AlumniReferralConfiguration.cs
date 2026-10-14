using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolERP.Domain.Student.Alumni.Entities;
using SchoolERP.Infrastructure.Persistence.Configurations.Common;

namespace SchoolERP.Infrastructure.Persistence.Configurations.Student.Alumni;

public class AlumniReferralConfiguration : BaseBranchOnlyEntityConfiguration<AlumniReferral>
{
    public override void Configure(EntityTypeBuilder<AlumniReferral> builder)
    {
        base.Configure(builder);
        builder.ToTable("AlumniReferrals");

        builder.Property(x => x.ReferredName).HasMaxLength(255).IsRequired();
        builder.Property(x => x.ReferredContact).HasMaxLength(20).IsRequired();
        builder.Property(x => x.ReferredEmail).HasMaxLength(255);
        builder.Property(x => x.Status).HasMaxLength(20).IsRequired().HasDefaultValue("pending");

        builder.HasOne(x => x.Alumni)
            .WithMany(a => a.Referrals)
            .HasForeignKey(x => x.AlumniId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.AlumniId);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.ReferredContact);

        builder.HasQueryFilter(x => !x.Alumni.IsDeleted);
    }
}
