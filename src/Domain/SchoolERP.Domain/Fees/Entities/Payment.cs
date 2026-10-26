using SchoolERP.Domain.Common;
using SchoolERP.Domain.Fees.Enums;

namespace SchoolERP.Domain.Fees.Entities;

/// <summary>
/// Immutable-ish payment record. Never hard-deleted — status transitions only.
/// Amount = AllocatedAmount + UnallocatedAmount.
/// Cheque-mode payments start as Pending; MarkChequeBounced reverts to Bounced.
/// </summary>
public sealed class Payment : BaseBranchEntity
{
    public long StudentId { get; set; }
    public string ReceiptNumber { get; set; } = string.Empty;
    public DateTime PaymentDate { get; set; }
    public decimal Amount { get; set; }
    public decimal AllocatedAmount { get; set; }
    public decimal UnallocatedAmount { get; set; }
    public PaymentSource PaymentSource { get; set; } = PaymentSource.Counter;
    public PaymentMode Mode { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? Notes { get; set; }
    public long? CollectedBy { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Completed;

    public ICollection<PaymentAllocation> Allocations { get; set; } = new List<PaymentAllocation>();
}
