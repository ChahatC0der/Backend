using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolERP.Application.Features.Fees.Builders;
using SchoolERP.Domain.Fees.Entities;
using SchoolERP.Domain.Fees.Enums;
using SchoolERP.Infrastructure.Persistence.Converters;

namespace SchoolERP.Infrastructure.Persistence.Configurations.Fees;

public sealed class ConcessionConfiguration : IEntityTypeConfiguration<Concession>
{
    public void Configure(EntityTypeBuilder<Concession> builder)
    {
        builder.ToTable("Concessions", t =>
        {
            // DB ke CHECKs se EXACT match — naam bhi, definition bhi, column bhi
            t.HasCheckConstraint("CK_Concessions_Category",
                "[Category] IN ('merit','sibling','staff','sports','other')");

            t.HasCheckConstraint("CHK_Concessions_CalculationType",
                "[CalculationType] IN ('fixed','percentage')");

            // NOTE: Value par koi CHECK nahi — DB me bhi nahi hai.
            // App validator (Value > 0, percentage <= 100) wahi enforce karta hai.
        });

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.Ignore(x => x.IsDeleted);
        builder.Property(x => x.TenantId).IsRequired();
        builder.Property(x => x.BranchId).IsRequired();

        builder.Property(x => x.Name).HasMaxLength(150).IsRequired();

        builder.Property(x => x.Category)
            .HasConversion(
                v => EnumMaps_JsonAdapter.ConcessionCategoryMap[v],
                v => EnumMaps_JsonAdapter.ConcessionCategoryMap
                      .First(kv => string.Equals(kv.Value, v, StringComparison.OrdinalIgnoreCase)).Key)
            .HasColumnName("Category")
            .HasMaxLength(40)
            .IsRequired();

        builder.Property(x => x.CalculationType)
            .HasConversion(
                v => EnumMaps_JsonAdapter.ConcessionCalculationTypeMap[v],
                v => EnumMaps_JsonAdapter.ConcessionCalculationTypeMap
                      .First(kv => string.Equals(kv.Value, v, StringComparison.OrdinalIgnoreCase)).Key)
            .HasColumnName("CalculationType")
            .HasMaxLength(40)
            .IsRequired();

        builder.Property(x => x.Value).HasPrecision(18, 2);
        builder.Property(x => x.FeeHeadId);
        builder.Property(x => x.IsAutoApply).HasDefaultValue(false);
        builder.Property(x => x.ApprovalRequired).HasDefaultValue(false);
        builder.Property(x => x.StartDate);
        builder.Property(x => x.EndDate);

        builder.Property(x => x.CreatedAt).IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property(x => x.UpdatedAt);
        builder.Property(x => x.DeletedAt);

        builder.HasIndex(x => new { x.TenantId, x.BranchId, x.IsAutoApply })
            .HasDatabaseName("IX_Concessions_Tenant_Branch_AutoApply")
            .HasFilter("[DeletedAt] IS NULL");
    }
}
