using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolERP.Domain.Fees.Entities;
using SchoolERP.Domain.Fees.Enums;
using SchoolERP.Infrastructure.Persistence.Converters;

namespace SchoolERP.Infrastructure.Persistence.Configurations.Fees;

public sealed class FeeHeadConfiguration : IEntityTypeConfiguration<FeeHead>
{
    public void Configure(EntityTypeBuilder<FeeHead> builder)
    {
        builder.ToTable("FeeHeads");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        // Physical dbo.FeeHeads has no IsDeleted column — ignore inherited property.
        builder.Ignore(x => x.IsDeleted);

        builder.Property(x => x.TenantId).IsRequired();
        builder.Property(x => x.BranchId).IsRequired();

        builder.Property(x => x.Name).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(500);

        builder.Property(x => x.IsRefundable).HasDefaultValue(false);
        builder.Property(x => x.IsMandatory).HasDefaultValue(true);
        builder.Property(x => x.IsOptional).HasDefaultValue(false);

        builder.Property(x => x.Status)
            .HasConversion(new EnumStringConverter<FeeHeadStatus>(EnumMaps.FeeHeadStatusMap))
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.CreatedAt).IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property(x => x.UpdatedAt);
        builder.Property(x => x.DeletedAt);

        builder.HasIndex(x => new { x.TenantId, x.BranchId, x.Code })
            .HasDatabaseName("UQ_FeeHeads_Tenant_Branch_Code")
            .IsUnique()
            .HasFilter("[DeletedAt] IS NULL");

        builder.HasIndex(x => new { x.TenantId, x.BranchId, x.Status })
            .HasDatabaseName("IX_FeeHeads_Tenant_Branch_Status");
    }
}
