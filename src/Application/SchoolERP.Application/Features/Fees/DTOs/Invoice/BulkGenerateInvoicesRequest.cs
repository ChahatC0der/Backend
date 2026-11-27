namespace SchoolERP.Application.Features.Fees.DTOs.Invoice;

/// <summary>
/// Filters define the target cohort. Cap = 500 students per request (Phase 1).
/// PeriodSequenceNumbers identifies one OR MANY billing cycles generated in a
/// single run, e.g. [3] or [1,2,3]. They are processed in ascending order
/// inside one transaction. PeriodLabel and DueDate are derived by the system
/// (PeriodLabelBuilder / period EndDate) and are NOT accepted from the request.
/// </summary>
public sealed class BulkGenerateInvoicesRequest
{
    public long AcademicYearId { get; set; }
    public long? ClassGroupId { get; set; }
    public long? ClassId { get; set; }
    public long? SectionId { get; set; }
    public List<int> PeriodSequenceNumbers { get; set; } = new();
}