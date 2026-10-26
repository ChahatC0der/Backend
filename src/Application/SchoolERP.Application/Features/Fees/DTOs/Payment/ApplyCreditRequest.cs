namespace SchoolERP.Application.Features.Fees.DTOs.Payment;

public sealed class ApplyCreditRequest
{
    public long InvoiceId { get; set; }
    public decimal AmountToApply { get; set; }
}
