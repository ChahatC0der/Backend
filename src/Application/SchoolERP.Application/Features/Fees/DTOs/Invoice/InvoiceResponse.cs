namespace SchoolERP.Application.Features.Fees.DTOs.Invoice;

public sealed class InvoiceResponse
{
    public long Id { get; set; }
    public long StudentId { get; set; }
    public long AcademicYearId { get; set; }
    public long ClassId { get; set; }
    public long? SectionId { get; set; }
    public long? FeeStructureId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public string PeriodLabel { get; set; } = string.Empty;

    // Billing period tracking — sequence is the period identity
    // (null = manual / ADMISSION invoice); dates are the frozen period span.
    public int? PeriodSequenceNumber { get; set; }
    public DateTime? PeriodStartDate { get; set; }
    public DateTime? PeriodEndDate { get; set; }

    public DateTime GenerationDate { get; set; }
    public DateTime DueDate { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal ConcessionAmount { get; set; }
    public decimal LateFeeAmount { get; set; }
    public decimal NetPayable { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal CreditAppliedAmount { get; set; }
    public decimal Outstanding { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Late-fee rule snapshot frozen at generation time (for audit/support visibility).
    public string? LateFeeModeSnapshot { get; set; }
    public string? InvoiceLateFeeTypeSnapshot { get; set; }
    public decimal? InvoiceLateFeeValueSnapshot { get; set; }
    public short? InvoiceLateFeeGraceDaysSnapshot { get; set; }
    public decimal? InvoiceLateFeeCapPercentSnapshot { get; set; }
    public string? InvoiceLateFeeSlabConfigSnapshot { get; set; }
    public decimal? InvoiceLateFeeCapBase { get; set; }

    public List<InvoiceLineResponse> Lines { get; set; } = new();
}