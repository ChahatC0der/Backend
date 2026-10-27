namespace SchoolERP.Application.Features.Fees.DTOs.Reports;

public sealed class OutstandingReportRow
{
    public long InvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public long StudentId { get; set; }
    public long ClassId { get; set; }
    public long? SectionId { get; set; }
    public string PeriodLabel { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public decimal NetPayable { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal CreditAppliedAmount { get; set; }
    public decimal Outstanding { get; set; }
    public string Status { get; set; } = string.Empty;
}
