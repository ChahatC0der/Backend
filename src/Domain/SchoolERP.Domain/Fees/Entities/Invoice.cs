using SchoolERP.Domain.Common;
using SchoolERP.Domain.Fees.Enums;

namespace SchoolERP.Domain.Fees.Entities;

/// <summary>
/// Branch-scoped invoice header.
/// Physical dbo.Invoices has DeletedAt only (no IsDeleted) — ignored in EF config.
/// FeeStructureId = source structure (null for pure ad-hoc / manual invoices).
/// Late fee is represented as separate InvoiceLine rows (IsLateFeeLine = true), NOT stored on header.
/// </summary>
public sealed class Invoice : BaseBranchEntity
{
    public long StudentId { get; set; }
    public long AcademicYearId { get; set; }
    public long ClassId { get; set; }
    public long? SectionId { get; set; }
    public long? FeeStructureId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public string PeriodLabel { get; set; } = string.Empty;
    // Identifies the billing period within the Academic Year.
    public int? PeriodSequenceNumber { get; set; }

    // Stores the exact billing period boundaries.
    public DateOnly? PeriodStartDate { get; set; }
    public DateOnly? PeriodEndDate { get; set; }

    public DateTime GenerationDate { get; set; }
    public DateTime DueDate { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal ConcessionAmount { get; set; }
    public decimal LateFeeAmount { get; set; }
    public decimal NetPayable { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal CreditAppliedAmount { get; set; }
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;

    // in Invoice.cs
    public string? LateFeeModeSnapshot { get; set; }              // null = treat as 'per_head' (legacy)
    public string? InvoiceLateFeeTypeSnapshot { get; set; }
    public decimal? InvoiceLateFeeValueSnapshot { get; set; }
    public short? InvoiceLateFeeGraceDaysSnapshot { get; set; }
    public decimal? InvoiceLateFeeCapPercentSnapshot { get; set; }
    public string? InvoiceLateFeeSlabConfigSnapshot { get; set; }
    public decimal? InvoiceLateFeeCapBase { get; set; }            // = NetPayable, frozen

    public ICollection<InvoiceLine> Lines { get; set; } = new List<InvoiceLine>();
}
