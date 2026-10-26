namespace SchoolERP.Application.Features.Fees.DTOs.Payment;

public sealed class InvoiceAllocationItem
{
    public long InvoiceId { get; set; }
    public decimal AmountToAllocate { get; set; }
}
