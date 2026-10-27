using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolERP.Domain.Transport.Entities;
using SchoolERP.Domain.Transport.Enums;
using SchoolERP.Infrastructure.Persistence.Converters;

namespace SchoolERP.Infrastructure.Persistence.Configurations.Transport;

public sealed class StopConfiguration : IEntityTypeConfiguration<Stop>
{
    public void Configure(EntityTypeBuilder<Stop> builder)
    {
        builder.ToTable("Stops");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.Ignore(x => x.DeletedAt);

        builder.Property(x => x.RouteId).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(255).IsRequired();
        builder.Property(x => x.StopOrder).IsRequired();
        builder.Property(x => x.Landmark).HasMaxLength(255);
        builder.Property(x => x.DistanceFromCampus).HasPrecision(5, 2);

        builder.Property(x => x.StopType)
            .HasConversion(new EnumStringConverter<StopType>(EnumMaps.StopTypeMap))
            .HasMaxLength(20).IsRequired();

        builder.Property(x => x.CreatedAt).IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property(x => x.UpdatedAt);
        builder.Property(x => x.IsDeleted).IsRequired().HasDefaultValue(false);

        builder.HasIndex(x => new { x.RouteId, x.StopOrder })
            .HasDatabaseName("UQ_Stops_Route_Order")
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

        builder.HasIndex(x => x.RouteId)
            .HasDatabaseName("IX_Stops_RouteId")
            .HasFilter("[IsDeleted] = 0");
    }
}
