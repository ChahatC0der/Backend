using SchoolERP.Domain.Common;

namespace SchoolERP.Domain.Fees.Entities;

/// <summary>
/// Immutable invoice line. Two kinds, distinguished by IsLateFeeLine:
///   IsLateFeeLine = false  -> SOURCE charge line. Carries frozen late-fee RULE snapshots + LateFeeCapBase.
///   IsLateFeeLine = true   -> DERIVED late-fee line. Amount = computed late fee, SourceInvoiceLineId points to source.
/// No double counting: invoice rollup sums source lines for TotalAmount/ConcessionAmount
/// and late-fee lines for LateFeeAmount. All snapshots frozen at generation, never re-read from FeeStructureLine.
/// </summary>
public sealed class InvoiceLine : IdEntity
{
    public long InvoiceId { get; set; }
    public long FeeHeadId { get; set; }
    public decimal Amount { get; set; }
    public decimal ConcessionApplied { get; set; }
    public decimal NetAmount { get; set; }
    public string FeeHeadNameSnapshot { get; set; } = string.Empty;

    public bool IsLateFeeLine { get; set; }
    public long? SourceInvoiceLineId { get; set; }

    // Frozen rule snapshots — only meaningful on source lines (IsLateFeeLine = false)
    public bool LateFeeApplicable { get; set; }
    public string? LateFeeTypeSnapshot { get; set; }
    public decimal? LateFeeValueSnapshot { get; set; }
    public int? LateFeeGraceDaysSnapshot { get; set; }
    public decimal? LateFeeCapPercentSnapshot { get; set; }
    public string? LateFeeSlabConfigSnapshot { get; set; }
    public decimal LateFeeCapBase { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
