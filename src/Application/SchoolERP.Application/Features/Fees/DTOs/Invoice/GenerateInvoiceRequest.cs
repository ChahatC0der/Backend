namespace SchoolERP.Application.Features.Fees.DTOs.Invoice;

public sealed class GenerateInvoiceRequest
{
    public long StudentId { get; set; }
    public long AcademicYearId { get; set; }
    public string PeriodLabel { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
}
