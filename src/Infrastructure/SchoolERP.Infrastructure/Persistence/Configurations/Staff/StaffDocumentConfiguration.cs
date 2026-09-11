using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolERP.Domain.Staff.Entities;
using SchoolERP.Infrastructure.Persistence.Configurations.Common;

namespace SchoolERP.Infrastructure.Persistence.Configurations.Staff;

public class StaffDocumentConfiguration : BaseEntityConfiguration<StaffDocument>
{
    public override void Configure(EntityTypeBuilder<StaffDocument> builder)
    {
        base.Configure(builder);

        builder.ToTable("StaffDocuments");

        builder.Property(x => x.DocumentType).HasMaxLength(50).IsRequired();
        builder.Property(x => x.FileUrl).HasMaxLength(500).IsRequired();
        builder.Property(x => x.VerificationStatus).HasMaxLength(20).IsRequired().HasDefaultValue("not_verified");

        // Relationship
        builder.HasOne(x => x.Staff)
            .WithMany(s => s.Documents)
            .HasForeignKey(x => x.StaffId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.StaffId);
        builder.HasIndex(x => x.ExpiryDate).HasFilter("[ExpiryDate] IS NOT NULL");
        builder.HasIndex(x => x.VerificationStatus);
    }
}