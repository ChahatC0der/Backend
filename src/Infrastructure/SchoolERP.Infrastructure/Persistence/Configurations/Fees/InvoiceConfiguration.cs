using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolERP.Domain.Fees.Entities;
using SchoolERP.Domain.Fees.Enums;
using SchoolERP.Infrastructure.Persistence.Converters;

namespace SchoolERP.Infrastructure.Persistence.Configurations.Fees;

public sealed class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("Invoices");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.Ignore(x => x.IsDeleted);
        builder.Property(x => x.TenantId).IsRequired();
        builder.Property(x => x.BranchId).IsRequired();

        builder.Property(x => x.StudentId).IsRequired();
        builder.Property(x => x.AcademicYearId).IsRequired();
        builder.Property(x => x.ClassId).IsRequired();
        builder.Property(x => x.SectionId);
        builder.Property(x => x.FeeStructureId);

        builder.Property(x => x.InvoiceNumber).HasMaxLength(50).IsRequired();
        builder.Property(x => x.PeriodLabel).HasMaxLength(100).IsRequired();
        builder.Property(x => x.GenerationDate).IsRequired();
        builder.Property(x => x.DueDate).IsRequired();

        builder.Property(x => x.TotalAmount).HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.ConcessionAmount).HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.LateFeeAmount).HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.NetPayable).HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.PaidAmount).HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.CreditAppliedAmount).HasPrecision(18, 2).IsRequired();

        builder.Property(x => x.Status)
            .HasConversion(new EnumStringConverter<InvoiceStatus>(EnumMaps.InvoiceStatusMap))
            .HasMaxLength(20).IsRequired();

        builder.Property(x => x.CreatedAt).IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property(x => x.UpdatedAt);
        builder.Property(x => x.DeletedAt);

        builder.HasMany(x => x.Lines)
            .WithOne()
            .HasForeignKey(l => l.InvoiceId)
            .OnDelete(DeleteBehavior.Restrict);

        // UQ: at most one non-cancelled invoice per (Student, AY, PeriodLabel)
        builder.HasIndex(x => new { x.StudentId, x.AcademicYearId, x.PeriodLabel })
            .HasDatabaseName("UQ_Invoices_StudentYearPeriod")
            .IsUnique()
            .HasFilter("[Status] <> 'cancelled' AND [DeletedAt] IS NULL");

        // UQ: invoice number unique per (Tenant, Branch)
        builder.HasIndex(x => new { x.TenantId, x.BranchId, x.InvoiceNumber })
            .HasDatabaseName("UQ_Invoices_Tenant_Branch_InvoiceNumber")
            .IsUnique();

        // Historical-edit guard: fast lookup of non-cancelled invoices per structure
        builder.HasIndex(x => x.FeeStructureId)
            .HasDatabaseName("IX_Invoices_FeeStructureId_Active")
            .HasFilter("[FeeStructureId] IS NOT NULL AND [Status] <> 'cancelled' AND [DeletedAt] IS NULL");

        // Student outstanding lookup
        builder.HasIndex(x => new { x.StudentId, x.AcademicYearId, x.Status })
            .HasDatabaseName("IX_Invoices_StudentId_AY_Status");
    }
}
