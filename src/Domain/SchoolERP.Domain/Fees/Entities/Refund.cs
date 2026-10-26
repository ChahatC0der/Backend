using SchoolERP.Domain.Common;
using SchoolERP.Domain.Fees.Enums;

namespace SchoolERP.Domain.Fees.Entities;

/// <summary>
/// Refund record. Tenant-scoped only (no BranchId, no soft-delete) per physical DB.
/// PaymentId = null  wallet refund (partial allowed, ledger debit on complete).
/// PaymentId set   payment-linked refund (full payment only in Phase 1).
/// </summary>
public sealed class Refund : BaseTenantEntity
{
    public long StudentId { get; set; }
    public long? PaymentId { get; set; }
    public decimal Amount { get; set; }
    public RefundReason Reason { get; set; }
    public PaymentMode PaymentMode { get; set; }
    public string? ReferenceNumber { get; set; }
    public RefundStatus Status { get; set; } = RefundStatus.Pending;
    public long? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? RejectionReason { get; set; }
}
