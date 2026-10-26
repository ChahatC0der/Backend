namespace SchoolERP.Application.Features.Fees.DTOs.Payment;

public sealed class CreditHistoryResponse
{
    public long StudentId { get; set; }
    public decimal CurrentBalance { get; set; }
    public List<CreditLedgerRow> Ledger { get; set; } = new();
}

public sealed class CreditLedgerRow
{
    public long Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public decimal Amount { get; set; }
    public decimal BalanceAfter { get; set; }
    public string TransactionType { get; set; } = string.Empty;
    public long? ReferenceInvoiceId { get; set; }
    public long? ReferencePaymentId { get; set; }
    public string? Reason { get; set; }
}
