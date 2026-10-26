using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolERP.Domain.Fees.Entities;
using SchoolERP.Domain.Fees.Enums;
using SchoolERP.Infrastructure.Persistence.Converters;

namespace SchoolERP.Infrastructure.Persistence.Configurations.Fees;

public sealed class FeeStructureConfiguration : IEntityTypeConfiguration<FeeStructure>
{
    public void Configure(EntityTypeBuilder<FeeStructure> builder)
    {
        builder.ToTable("FeeStructures");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.Ignore(x => x.IsDeleted);         // no IsDeleted column in physical DB
        builder.Property(x => x.BranchId).IsRequired();
        builder.Property(x => x.AcademicYearId).IsRequired();

        builder.Property(x => x.Name).HasMaxLength(150).IsRequired();
        builder.Property(x => x.ApplicableFrom).IsRequired();
        builder.Property(x => x.ApplicableTo).IsRequired();

        builder.Property(x => x.ProrationMethod)
            .HasConversion(new EnumStringConverter<ProrationMethod>(EnumMaps.ProrationMethodMap))
            .HasMaxLength(20).IsRequired();

        builder.Property(x => x.PaymentAllocationPriority)
            .HasConversion(new EnumStringConverter<PaymentAllocationPriority>(EnumMaps.PaymentAllocationPriorityMap))
            .HasMaxLength(20).IsRequired();

        builder.Property(x => x.Status)
            .HasConversion(new EnumStringConverter<FeeStructureStatus>(EnumMaps.FeeStructureStatusMap))
            .HasMaxLength(20).IsRequired();

        builder.Property(x => x.CreatedAt).IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property(x => x.UpdatedAt);
        builder.Property(x => x.DeletedAt);

        builder.HasMany(x => x.Lines)
            .WithOne()
            .HasForeignKey(l => l.FeeStructureId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.BranchId, x.AcademicYearId, x.Status })
            .HasDatabaseName("IX_FeeStructures_Branch_AY_Status");

        builder.HasIndex(x => new { x.BranchId, x.AcademicYearId, x.ClassGroupId, x.ClassId, x.SectionId })
            .HasDatabaseName("IX_FeeStructures_Scope")
            .HasFilter("[DeletedAt] IS NULL");
    }
}
