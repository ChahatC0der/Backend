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
    public DateTime GenerationDate { get; set; }
    public DateTime DueDate { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal ConcessionAmount { get; set; }
    public decimal LateFeeAmount { get; set; }
    public decimal NetPayable { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal CreditAppliedAmount { get; set; }
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;

    public ICollection<InvoiceLine> Lines { get; set; } = new List<InvoiceLine>();
}
