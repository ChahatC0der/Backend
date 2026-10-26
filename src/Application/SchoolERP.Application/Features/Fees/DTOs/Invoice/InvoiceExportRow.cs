namespace SchoolERP.Application.Features.Fees.DTOs.Invoice;

public sealed class InvoiceExportRow
{
    public long Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public long StudentId { get; set; }
    public string PeriodLabel { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal ConcessionAmount { get; set; }
    public decimal LateFeeAmount { get; set; }
    public decimal NetPayable { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal Outstanding { get; set; }
    public string Status { get; set; } = string.Empty;
}
