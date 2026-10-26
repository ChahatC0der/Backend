namespace SchoolERP.Application.Features.Fees.Builders;

/// <summary>In-memory invoice blueprint — persisted by the command handler.</summary>
public sealed class InvoiceDraft
{
    public long StudentId { get; init; }
    public long AcademicYearId { get; init; }
    public long ClassId { get; init; }
    public long? SectionId { get; init; }
    public long? FeeStructureId { get; init; }
    public string PeriodLabel { get; init; } = string.Empty;
    public DateTime GenerationDate { get; init; }
    public DateTime DueDate { get; init; }
    public decimal TotalAmount { get; init; }
    public decimal ConcessionAmount { get; init; }
    public decimal NetPayable { get; init; }
    public List<InvoiceLineDraft> Lines { get; init; } = new();
    public List<ConcessionUsageDraft> ConcessionUsages { get; init; } = new();
}

public sealed class InvoiceLineDraft
{
    public long FeeHeadId { get; init; }
    public string FeeHeadNameSnapshot { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public decimal ConcessionApplied { get; init; }
    public decimal NetAmount { get; init; }
    public bool LateFeeApplicable { get; init; }
    public string? LateFeeTypeSnapshot { get; init; }
    public decimal? LateFeeValueSnapshot { get; init; }
    public int? LateFeeGraceDaysSnapshot { get; init; }
    public decimal? LateFeeCapPercentSnapshot { get; init; }
    public string? LateFeeSlabConfigSnapshot { get; init; }
    public decimal LateFeeCapBase { get; init; }
}

public sealed class ConcessionUsageDraft
{
    public long ConcessionAllocationId { get; init; }
    public decimal AppliedAmount { get; init; }
}
