namespace SchoolERP.Application.Features.Fees.DTOs.Refund;

public sealed class RequestRefundRequest
{
    public long StudentId { get; set; }
    public long? PaymentId { get; set; }
    public decimal Amount { get; set; }
    public string Reason { get; set; } = "other";
    public string PaymentMode { get; set; } = "bank_transfer";
    public string? ReferenceNumber { get; set; }
}
