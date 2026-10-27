using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolERP.Domain.Transport.Entities;

namespace SchoolERP.Infrastructure.Persistence.Configurations.Transport;

public sealed class DriverAssignmentConfiguration : IEntityTypeConfiguration<DriverAssignment>
{
    public void Configure(EntityTypeBuilder<DriverAssignment> builder)
    {
        builder.ToTable("DriverAssignments");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.Property(x => x.VehicleId).IsRequired();
        builder.Property(x => x.DriverStaffId).IsRequired();
        builder.Property(x => x.ValidFrom).IsRequired();
        builder.Property(x => x.ValidTo);
        builder.Property(x => x.IsCurrent).IsRequired().HasDefaultValue(false);
        builder.Property(x => x.CreatedAt).IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property(x => x.UpdatedAt);
        builder.Property(x => x.IsDeleted).IsRequired().HasDefaultValue(false);

        // One vehicle can have only one current driver
        builder.HasIndex(x => x.VehicleId)
            .HasDatabaseName("UQ_DriverAssignments_VehicleId_Current")
            .IsUnique()
            .HasFilter("[IsCurrent] = 1 AND [IsDeleted] = 0");

        // Driver history lookup
        builder.HasIndex(x => new { x.DriverStaffId, x.IsCurrent })
            .HasDatabaseName("IX_DriverAssignments_Driver_Current")
            .HasFilter("[IsDeleted] = 0");
    }
}
