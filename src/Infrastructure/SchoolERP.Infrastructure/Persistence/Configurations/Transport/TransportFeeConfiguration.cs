using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolERP.Domain.Transport.Entities;
using SchoolERP.Domain.Transport.Enums;
using SchoolERP.Infrastructure.Persistence.Converters;

namespace SchoolERP.Infrastructure.Persistence.Configurations.Transport;

public sealed class TransportFeeConfiguration : IEntityTypeConfiguration<TransportFee>
{
    public void Configure(EntityTypeBuilder<TransportFee> builder)
    {
        builder.ToTable("TransportFees");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.Ignore(x => x.DeletedAt);

        builder.Property(x => x.BranchId).IsRequired();
        builder.Property(x => x.AcademicYearId).IsRequired();
        builder.Property(x => x.RouteId);
        builder.Property(x => x.StopId);
        builder.Property(x => x.FeeAmount).HasPrecision(18, 2).IsRequired();

        builder.Property(x => x.Frequency)
            .HasConversion(new EnumStringConverter<TransportFeeFrequency>(EnumMaps.TransportFeeFrequencyMap))
            .HasMaxLength(20).IsRequired();

        builder.Property(x => x.CreatedAt).IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property(x => x.UpdatedAt);
        builder.Property(x => x.IsDeleted).IsRequired().HasDefaultValue(false);

        builder.HasIndex(x => new { x.AcademicYearId, x.RouteId })
            .HasDatabaseName("IX_TransportFees_Year_Route")
            .HasFilter("[RouteId] IS NOT NULL AND [IsDeleted] = 0");

        builder.HasIndex(x => new { x.AcademicYearId, x.StopId })
            .HasDatabaseName("IX_TransportFees_Year_Stop")
            .HasFilter("[StopId] IS NOT NULL AND [IsDeleted] = 0");
    }
}
