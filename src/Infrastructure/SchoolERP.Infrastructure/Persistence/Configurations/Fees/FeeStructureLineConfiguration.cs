using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolERP.Domain.Fees.Entities;
using SchoolERP.Domain.Fees.Enums;
using SchoolERP.Infrastructure.Persistence.Converters;

namespace SchoolERP.Infrastructure.Persistence.Configurations.Fees;

public sealed class FeeStructureLineConfiguration : IEntityTypeConfiguration<FeeStructureLine>
{
    public void Configure(EntityTypeBuilder<FeeStructureLine> builder)
    {
        builder.ToTable("FeeStructureLines");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.Ignore(x => x.IsDeleted);

        builder.Property(x => x.FeeStructureId).IsRequired();
        builder.Property(x => x.FeeHeadId).IsRequired();
        builder.Property(x => x.Amount).HasPrecision(18, 2).IsRequired();

        builder.Property(x => x.Frequency)
            .HasConversion(new EnumStringConverter<FeeFrequency>(EnumMaps.FeeFrequencyMap))
            .HasMaxLength(20).IsRequired();

        builder.Property(x => x.DueDay).HasColumnType("smallint");
        builder.Property(x => x.LateFeeApplicable).HasDefaultValue(false);

        builder.Property(x => x.LateFeeType)
            .HasConversion(new EnumStringConverter<LateFeeType>(EnumMaps.LateFeeTypeMap))
            .HasMaxLength(20);

        builder.Property(x => x.LateFeeValue).HasPrecision(18, 2);
        builder.Property(x => x.LateFeeGraceDays).HasColumnType("smallint");
        builder.Property(x => x.LateFeeCapPercent).HasPrecision(5, 2);
        builder.Property(x => x.LateFeeSlabConfig).HasColumnType("nvarchar(max)");

        builder.Property(x => x.CreatedAt).IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property(x => x.UpdatedAt);
        builder.Property(x => x.DeletedAt);

        builder.HasIndex(x => x.FeeStructureId)
            .HasDatabaseName("IX_FeeStructureLines_Structure")
            .HasFilter("[DeletedAt] IS NULL");
    }
}
