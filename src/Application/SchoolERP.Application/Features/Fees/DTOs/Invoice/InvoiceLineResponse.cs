namespace SchoolERP.Application.Features.Fees.DTOs.Invoice;

public sealed class InvoiceLineResponse
{
    public long Id { get; set; }
    public long FeeHeadId { get; set; }
    public string FeeHeadName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal ConcessionApplied { get; set; }
    public decimal NetAmount { get; set; }
    public bool IsLateFeeLine { get; set; }
    public long? SourceInvoiceLineId { get; set; }
}
