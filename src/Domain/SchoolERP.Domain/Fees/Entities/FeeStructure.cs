using SchoolERP.Domain.Common;
using SchoolERP.Domain.Fees.Enums;



namespace SchoolERP.Domain.Fees.Entities;

/// <summary>
/// Branch-scoped (BranchEntity — no direct TenantId; validate Branch.TenantId in handlers).
/// Physical dbo.FeeStructures has DeletedAt only (no IsDeleted).
/// </summary>
public sealed class FeeStructure : BranchEntity
{
    public long AcademicYearId { get; set; }
    public long? ClassGroupId { get; set; }
    public long? ClassId { get; set; }
    public long? SectionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public LateFeeMode LateFeeMode { get; set; } = LateFeeMode.PerHead;
    public DateTime ApplicableFrom { get; set; }
    public DateTime ApplicableTo { get; set; }
    public ProrationMethod ProrationMethod { get; set; } = ProrationMethod.FullMonth;
    public PaymentAllocationPriority PaymentAllocationPriority { get; set; } = PaymentAllocationPriority.ManualOnly;
    public FeeStructureStatus Status { get; set; } = FeeStructureStatus.Draft;
    // Whole-invoice rule (only used when LateFeeMode == WholeInvoice)
    public string? InvoiceLateFeeType { get; set; }              // 'stagnant_fixed' | 'graduated' | 'monthly'
    public decimal? InvoiceLateFeeValue { get; set; }
    public short? InvoiceLateFeeGraceDays { get; set; }
    public decimal? InvoiceLateFeeCapPercent { get; set; }
    public string? InvoiceLateFeeSlabConfig { get; set; }        // JSON, validated via ISJSON in DB

    public ICollection<FeeStructureLine> Lines { get; set; } = new List<FeeStructureLine>();
}
