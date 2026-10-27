namespace SchoolERP.Application.Features.Fees.DTOs.Reports;

public sealed class DefaulterReportRow
{
    public long InvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public long StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string EnrollmentId { get; set; } = string.Empty;
    public long ClassId { get; set; }
    public long? SectionId { get; set; }
    public DateTime DueDate { get; set; }
    public int DaysOverdue { get; set; }
    public decimal Outstanding { get; set; }
}
