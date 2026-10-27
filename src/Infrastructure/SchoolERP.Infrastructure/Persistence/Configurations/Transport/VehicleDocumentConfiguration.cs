using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolERP.Domain.Transport.Entities;
using SchoolERP.Domain.Transport.Enums;
using SchoolERP.Infrastructure.Persistence.Converters;

namespace SchoolERP.Infrastructure.Persistence.Configurations.Transport;

public sealed class VehicleDocumentConfiguration : IEntityTypeConfiguration<VehicleDocument>
{
    public void Configure(EntityTypeBuilder<VehicleDocument> builder)
    {
        builder.ToTable("VehicleDocuments");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.Property(x => x.VehicleId).IsRequired();

        builder.Property(x => x.DocumentType)
            .HasConversion(new EnumStringConverter<VehicleDocumentType>(EnumMaps.VehicleDocumentTypeMap))
            .HasMaxLength(50).IsRequired();

        builder.Property(x => x.FileUrl).HasMaxLength(500).IsRequired();
        builder.Property(x => x.ExpiryDate);
        builder.Property(x => x.UploadedBy);
        builder.Property(x => x.CreatedAt).IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property(x => x.IsDeleted).IsRequired().HasDefaultValue(false);

        builder.HasIndex(x => new { x.VehicleId, x.DocumentType })
            .HasDatabaseName("UQ_VehicleDocuments_Vehicle_Type")
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

        builder.HasIndex(x => x.VehicleId)
            .HasDatabaseName("IX_VehicleDocuments_VehicleId")
            .HasFilter("[IsDeleted] = 0");
    }
}
