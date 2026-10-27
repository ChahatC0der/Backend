using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolERP.Domain.Transport.Entities;
using SchoolERP.Domain.Transport.Enums;
using SchoolERP.Infrastructure.Persistence.Converters;

namespace SchoolERP.Infrastructure.Persistence.Configurations.Transport;

public sealed class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
{
    public void Configure(EntityTypeBuilder<Vehicle> builder)
    {
        builder.ToTable("Vehicles");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        // DB has IsDeleted (not DeletedAt) — ignore the DeletedAt base property
        builder.Ignore(x => x.DeletedAt);

        builder.Property(x => x.TenantId).IsRequired();
        builder.Property(x => x.BranchId).IsRequired();

        builder.Property(x => x.VehicleNumber).HasMaxLength(50).IsRequired();

        builder.Property(x => x.VehicleType)
            .HasConversion(new EnumStringConverter<VehicleType>(EnumMaps.VehicleTypeMap))
            .HasMaxLength(20).IsRequired();

        builder.Property(x => x.Capacity).IsRequired();
        builder.Property(x => x.Model).HasMaxLength(100);
        builder.Property(x => x.Make).HasMaxLength(100);
        builder.Property(x => x.Year);
        builder.Property(x => x.InsuranceValidUntil);
        builder.Property(x => x.PucValidUntil);
        builder.Property(x => x.FitnessValidUntil);

        builder.Property(x => x.Status)
            .HasConversion(new EnumStringConverter<VehicleStatus>(EnumMaps.VehicleStatusMap))
            .HasMaxLength(20).IsRequired();

        builder.Property(x => x.CreatedAt).IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property(x => x.UpdatedAt);
        builder.Property(x => x.IsDeleted).IsRequired().HasDefaultValue(false);

        builder.HasIndex(x => new { x.TenantId, x.BranchId, x.VehicleNumber })
            .HasDatabaseName("UQ_Vehicles_Tenant_Branch_Number")
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

        builder.HasIndex(x => new { x.BranchId, x.Status })
            .HasDatabaseName("IX_Vehicles_Branch_Status")
            .HasFilter("[IsDeleted] = 0");

        builder.HasIndex(x => x.InsuranceValidUntil)
            .HasDatabaseName("IX_Vehicles_InsuranceExpiry")
            .HasFilter("[InsuranceValidUntil] IS NOT NULL AND [IsDeleted] = 0");
    }
}
