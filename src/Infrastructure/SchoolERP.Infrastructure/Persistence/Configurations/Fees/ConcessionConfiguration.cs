using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolERP.Domain.Fees.Entities;
using SchoolERP.Domain.Fees.Enums;
using SchoolERP.Infrastructure.Persistence.Converters;

namespace SchoolERP.Infrastructure.Persistence.Configurations.Fees;

public sealed class ConcessionConfiguration : IEntityTypeConfiguration<Concession>
{
    public void Configure(EntityTypeBuilder<Concession> builder)
    {
        builder.ToTable("Concessions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.Ignore(x => x.IsDeleted);
        builder.Property(x => x.TenantId).IsRequired();
        builder.Property(x => x.BranchId).IsRequired();

        builder.Property(x => x.Name).HasMaxLength(150).IsRequired();

        builder.Property(x => x.Type)
            .HasConversion(new EnumStringConverter<ConcessionType>(EnumMaps.ConcessionTypeMap))
            .HasMaxLength(20).IsRequired();

        builder.Property(x => x.Value).HasPrecision(18, 2).IsRequired();
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
