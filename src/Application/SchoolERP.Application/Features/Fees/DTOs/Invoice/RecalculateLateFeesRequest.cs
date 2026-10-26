namespace SchoolERP.Application.Features.Fees.DTOs.Invoice;

/// <summary>Scoped recalc. If InvoiceId provided, recalc just that invoice; else all overdue in scope.</summary>
public sealed class RecalculateLateFeesRequest
{
    public long? InvoiceId { get; set; }
    public long? AcademicYearId { get; set; }
    public bool DryRun { get; set; }
}
