namespace SchoolERP.Application.Features.Fees.DTOs.Invoice;

/// <summary>
/// Filters define the target cohort. Cap = 500 students per request (Phase 1).
/// periodLabel identifies the billing cycle (e.g. "Apr 2025", "Q1 (Apr-Jun)").
/// </summary>
public sealed class BulkGenerateInvoicesRequest
{
    public long AcademicYearId { get; set; }
    public long? ClassGroupId { get; set; }
    public long? ClassId { get; set; }
    public long? SectionId { get; set; }
    public string PeriodLabel { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
}
