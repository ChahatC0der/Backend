namespace SchoolERP.Application.Features.Fees.DTOs.Payment;

public sealed class ReceiptResponse
{
    public PaymentResponse Payment { get; set; } = null!;
    public List<ReceiptAllocationLine> Allocations { get; set; } = new();
    public decimal WalletBalanceAfter { get; set; }
}

public sealed class ReceiptAllocationLine
{
    public long InvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public decimal AllocatedAmount { get; set; }
}
