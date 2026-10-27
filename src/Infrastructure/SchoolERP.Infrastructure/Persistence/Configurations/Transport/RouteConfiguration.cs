using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolERP.Domain.Transport.Entities;
using SchoolERP.Domain.Transport.Enums;
using SchoolERP.Infrastructure.Persistence.Converters;

namespace SchoolERP.Infrastructure.Persistence.Configurations.Transport;

public sealed class RouteConfiguration : IEntityTypeConfiguration<Route>
{
    public void Configure(EntityTypeBuilder<Route> builder)
    {
        builder.ToTable("Routes");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.Ignore(x => x.DeletedAt);

        builder.Property(x => x.BranchId).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Code).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(255);

        builder.Property(x => x.Status)
            .HasConversion(new EnumStringConverter<RouteStatus>(EnumMaps.RouteStatusMap))
            .HasMaxLength(20).IsRequired();

        builder.Property(x => x.CreatedAt).IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property(x => x.UpdatedAt);
        builder.Property(x => x.IsDeleted).IsRequired().HasDefaultValue(false);

        builder.HasMany(x => x.Stops)
            .WithOne()
            .HasForeignKey(s => s.RouteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.BranchId, x.Code })
            .HasDatabaseName("UQ_Routes_Branch_Code")
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

        builder.HasIndex(x => new { x.BranchId, x.Status })
            .HasDatabaseName("IX_Routes_Branch_Status")
            .HasFilter("[IsDeleted] = 0");
    }
}
