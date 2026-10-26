using PaymentEntity = SchoolERP.Domain.Fees.Entities.Payment;

namespace SchoolERP.Application.Features.Fees.DTOs.Payment;

public sealed class PaymentResponse
{
    public long Id { get; set; }
    public long StudentId { get; set; }
    public string ReceiptNumber { get; set; } = string.Empty;
    public DateTime PaymentDate { get; set; }
    public decimal Amount { get; set; }
    public decimal AllocatedAmount { get; set; }
    public decimal UnallocatedAmount { get; set; }
    public string Mode { get; set; } = string.Empty;
    public string PaymentSource { get; set; } = string.Empty;
    public string? ReferenceNumber { get; set; }
    public string? Notes { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public static PaymentResponse FromEntity(PaymentEntity p) => new()
    {
        Id = p.Id,
        StudentId = p.StudentId,
        ReceiptNumber = p.ReceiptNumber,
        PaymentDate = p.PaymentDate,
        Amount = p.Amount,
        AllocatedAmount = p.AllocatedAmount,
        UnallocatedAmount = p.UnallocatedAmount,
        Mode = p.Mode.ToString(),
        PaymentSource = p.PaymentSource.ToString(),
        ReferenceNumber = p.ReferenceNumber,
        Notes = p.Notes,
        Status = p.Status.ToString(),
        CreatedAt = p.CreatedAt,
    };
}
