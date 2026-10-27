using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolERP.Domain.Transport.Entities;

namespace SchoolERP.Infrastructure.Persistence.Configurations.Transport;

public sealed class TransportAssignmentConfiguration : IEntityTypeConfiguration<TransportAssignment>
{
    public void Configure(EntityTypeBuilder<TransportAssignment> builder)
    {
        builder.ToTable("TransportAssignments");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.Ignore(x => x.DeletedAt);

        builder.Property(x => x.TenantId).IsRequired();
        builder.Property(x => x.StudentId).IsRequired();
        builder.Property(x => x.AcademicYearId).IsRequired();

        builder.Property(x => x.PickupVehicleId).IsRequired();
        builder.Property(x => x.PickupRouteId).IsRequired();
        builder.Property(x => x.PickupStopId).IsRequired();

        builder.Property(x => x.DropVehicleId);
        builder.Property(x => x.DropRouteId);
        builder.Property(x => x.DropStopId);

        builder.Property(x => x.ValidFrom).IsRequired();
        builder.Property(x => x.ValidTo);
        builder.Property(x => x.IsCurrent).IsRequired().HasDefaultValue(false);

        builder.Property(x => x.CreatedAt).IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property(x => x.UpdatedAt);
        builder.Property(x => x.IsDeleted).IsRequired().HasDefaultValue(false);

        // One active transport per student per year
        builder.HasIndex(x => new { x.StudentId, x.AcademicYearId })
            .HasDatabaseName("UQ_TransportAssignments_Student_Year_Current")
            .IsUnique()
            .HasFilter("[IsCurrent] = 1 AND [IsDeleted] = 0");

        builder.HasIndex(x => new { x.PickupRouteId, x.IsCurrent })
            .HasDatabaseName("IX_TransportAssignments_Route_Current")
            .HasFilter("[IsDeleted] = 0");

        builder.HasIndex(x => new { x.PickupVehicleId, x.IsCurrent })
            .HasDatabaseName("IX_TransportAssignments_Vehicle_Current")
            .HasFilter("[IsDeleted] = 0");
    }
}
