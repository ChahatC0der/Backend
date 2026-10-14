using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolERP.Domain.Student.Registration.Entities;
using SchoolERP.Infrastructure.Persistence.Configurations.Common;

namespace SchoolERP.Infrastructure.Persistence.Configurations.Student.Registration;

public class RegistrationDocumentConfiguration : BaseEntityConfiguration<RegistrationDocument>
{
    public override void Configure(EntityTypeBuilder<RegistrationDocument> builder)
    {
        base.Configure(builder);

        builder.ToTable("RegistrationDocuments");

        builder.Property(x => x.DocumentType).HasMaxLength(50).IsRequired();
        builder.Property(x => x.FileUrl).HasMaxLength(500);
        builder.Property(x => x.Description).HasMaxLength(255);
        builder.Property(x => x.Status).HasMaxLength(20).IsRequired().HasDefaultValue("not_uploaded");
        builder.Property(x => x.RejectionReason).HasColumnType("nvarchar(max)");

        builder.HasOne(x => x.Registration)
            .WithMany(r => r.Documents)
            .HasForeignKey(x => x.RegistrationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.RegistrationId);
        builder.HasIndex(x => x.Status);

        // Matching filter to avoid required-end warning
        builder.HasQueryFilter(x => !x.Registration.IsDeleted);
    }
}
