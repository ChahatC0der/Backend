namespace SchoolERP.Application.Features.Fees.DTOs.FeeStructure;

public sealed class CreateFeeStructureRequest
{
    public long AcademicYearId { get; set; }
    public long? ClassGroupId { get; set; }
    public long? ClassId { get; set; }
    public long? SectionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime ApplicableFrom { get; set; }
    public DateTime ApplicableTo { get; set; }
    public string ProrationMethod { get; set; } = "full_month";
    public string PaymentAllocationPriority { get; set; } = "manual_only";

    // ── Late fee (parent-level, whole-invoice mode) ──
    public string LateFeeMode { get; set; } = "per_head";   // none | per_head | whole_invoice
    public string? InvoiceLateFeeType { get; set; }          // required when LateFeeMode = whole_invoice
    public decimal? InvoiceLateFeeValue { get; set; }         // required when LateFeeMode = whole_invoice
    public short? InvoiceLateFeeGraceDays { get; set; }       // required when LateFeeMode = whole_invoice
    public decimal? InvoiceLateFeeCapPercent { get; set; }    // optional even in whole_invoice mode
    public string? InvoiceLateFeeSlabConfig { get; set; }     // required when InvoiceLateFeeType = graduated

    public List<FeeStructureLineDto> Lines { get; set; } = new();
}