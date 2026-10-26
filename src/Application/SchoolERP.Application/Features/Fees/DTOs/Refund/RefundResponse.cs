using RefundEntity = SchoolERP.Domain.Fees.Entities.Refund;

namespace SchoolERP.Application.Features.Fees.DTOs.Refund;

public sealed class RefundResponse
{
    public long Id { get; set; }
    public long StudentId { get; set; }
    public long? PaymentId { get; set; }
    public decimal Amount { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string PaymentMode { get; set; } = string.Empty;
    public string? ReferenceNumber { get; set; }
    public string Status { get; set; } = string.Empty;
    public long? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public static RefundResponse FromEntity(RefundEntity r) => new()
    {
        Id = r.Id,
        StudentId = r.StudentId,
        PaymentId = r.PaymentId,
        Amount = r.Amount,
        Reason = r.Reason.ToString(),
        PaymentMode = r.PaymentMode.ToString(),
        ReferenceNumber = r.ReferenceNumber,
        Status = r.Status.ToString(),
        ApprovedBy = r.ApprovedBy,
        ApprovedAt = r.ApprovedAt,
        RejectionReason = r.RejectionReason,
        CreatedAt = r.CreatedAt,
        UpdatedAt = r.UpdatedAt,
    };
}
