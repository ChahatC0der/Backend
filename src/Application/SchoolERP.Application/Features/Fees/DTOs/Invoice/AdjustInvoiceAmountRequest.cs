namespace SchoolERP.Application.Features.Fees.DTOs.Invoice;

public sealed class AdjustInvoiceAmountRequest
{
    public long InvoiceLineId { get; set; }
    public decimal NewAmount { get; set; }
    public string Reason { get; set; } = string.Empty;
}
