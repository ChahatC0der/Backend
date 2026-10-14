using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolERP.Domain.Student.Alumni.Entities;
using SchoolERP.Infrastructure.Persistence.Configurations.Common;

namespace SchoolERP.Infrastructure.Persistence.Configurations.Student.Alumni;

public class AlumniDonationConfiguration : BaseEntityConfiguration<AlumniDonation>
{
    public override void Configure(EntityTypeBuilder<AlumniDonation> builder)
    {
        base.Configure(builder);
        builder.ToTable("AlumniDonations");

        builder.Property(x => x.Amount).HasColumnType("decimal(12,2)").IsRequired();
        builder.Property(x => x.Purpose).HasMaxLength(50).IsRequired();
        builder.Property(x => x.PaymentMode).HasMaxLength(20).IsRequired();
        builder.Property(x => x.ReceiptNumber).HasMaxLength(50).IsRequired();
        builder.HasIndex(x => x.ReceiptNumber).IsUnique();
        builder.Property(x => x.Notes).HasColumnType("nvarchar(max)");

        builder.HasOne(x => x.Alumni)
            .WithMany(a => a.Donations)
            .HasForeignKey(x => x.AlumniId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.AlumniId);
        builder.HasIndex(x => x.DonationDate);

        // Matching filter (avoid warning)
        builder.HasQueryFilter(x => !x.Alumni.IsDeleted);
    }
}
