using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolERP.Domain.Transport.Entities;

namespace SchoolERP.Infrastructure.Persistence.Configurations.Transport;

public sealed class RouteVehicleConfiguration : IEntityTypeConfiguration<RouteVehicle>
{
    public void Configure(EntityTypeBuilder<RouteVehicle> builder)
    {
        builder.ToTable("RouteVehicles");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.Property(x => x.RouteId).IsRequired();
        builder.Property(x => x.VehicleId).IsRequired();
        builder.Property(x => x.ValidFrom).IsRequired();
        builder.Property(x => x.ValidTo);
        builder.Property(x => x.IsCurrent).IsRequired().HasDefaultValue(false);
        builder.Property(x => x.CreatedAt).IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property(x => x.UpdatedAt);
        builder.Property(x => x.IsDeleted).IsRequired().HasDefaultValue(false);

        // One vehicle can be current on only one route
        builder.HasIndex(x => x.VehicleId)
            .HasDatabaseName("UQ_RouteVehicles_VehicleId_Current")
            .IsUnique()
            .HasFilter("[IsCurrent] = 1 AND [IsDeleted] = 0");

        // Route lookup of current vehicles
        builder.HasIndex(x => new { x.RouteId, x.IsCurrent })
            .HasDatabaseName("IX_RouteVehicles_Route_Current")
            .HasFilter("[IsDeleted] = 0");
    }
}
