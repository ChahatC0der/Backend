namespace SchoolERP.Domain.Fees.Entities;

/// <summary>
/// Composite PK (PaymentId, InvoiceId). Immutable once written — no soft delete, no updates.
/// </summary>
public sealed class PaymentAllocation
{
    public long PaymentId { get; set; }
    public long InvoiceId { get; set; }
    public decimal AllocatedAmount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
