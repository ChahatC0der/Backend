namespace SchoolERP.Application.Features.Fees.DTOs.FeeStructure;

public sealed class UpdateFeeStructureRequest
{
    public string Name { get; set; } = string.Empty;
    public DateTime ApplicableFrom { get; set; }
    public DateTime ApplicableTo { get; set; }
    public string ProrationMethod { get; set; } = "full_month";
    public string PaymentAllocationPriority { get; set; } = "manual_only";

    // ── Late fee (parent-level, whole-invoice mode) ──
    public string LateFeeMode { get; set; } = "per_head";
    public string? InvoiceLateFeeType { get; set; }
    public decimal? InvoiceLateFeeValue { get; set; }
    public short? InvoiceLateFeeGraceDays { get; set; }
    public decimal? InvoiceLateFeeCapPercent { get; set; }
    public string? InvoiceLateFeeSlabConfig { get; set; }

    public List<FeeStructureLineDto> Lines { get; set; } = new();
}