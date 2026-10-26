namespace SchoolERP.Application.Features.Fees.DTOs.Refund;

public sealed class RefundExportRow
{
    public long Id { get; set; }
    public long StudentId { get; set; }
    public long? PaymentId { get; set; }
    public decimal Amount { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string PaymentMode { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
