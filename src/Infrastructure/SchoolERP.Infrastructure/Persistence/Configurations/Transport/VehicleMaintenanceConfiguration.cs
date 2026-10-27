using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolERP.Domain.Transport.Entities;

namespace SchoolERP.Infrastructure.Persistence.Configurations.Transport;

public sealed class VehicleMaintenanceConfiguration : IEntityTypeConfiguration<VehicleMaintenance>
{
    public void Configure(EntityTypeBuilder<VehicleMaintenance> builder)
    {
        builder.ToTable("VehicleMaintenance");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.Property(x => x.VehicleId).IsRequired();
        builder.Property(x => x.MaintenanceDate).IsRequired();
        builder.Property(x => x.Description).HasColumnType("nvarchar(max)");
        builder.Property(x => x.Cost).HasPrecision(18, 2);
        builder.Property(x => x.OdometerReading);
        builder.Property(x => x.CreatedAt).IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property(x => x.IsDeleted).IsRequired().HasDefaultValue(false);

        builder.HasIndex(x => new { x.VehicleId, x.MaintenanceDate })
            .HasDatabaseName("IX_VehicleMaintenance_Vehicle_Date")
            .IsDescending(false, true)
            .HasFilter("[IsDeleted] = 0");
    }
}
