using SchoolERP.Domain.Fees.Enums;

namespace SchoolERP.Infrastructure.Persistence.Converters;

/// <summary>
/// Exact DB string values for every enum in the Fees module.
/// Field names carry the "Map" suffix so they never shadow the enum type
/// (e.g. FeeHeadStatusMap vs. the enum type FeeHeadStatus).
///
/// VERIFY EACH STRING against SSMS:
///     SELECT DISTINCT <ColumnName> FROM dbo.<TableName>;
/// Any mismatch here will break filtered indexes and raw SQL reports.
/// </summary>
public static class EnumMaps
{
    public static readonly IReadOnlyDictionary<FeeHeadStatus, string> FeeHeadStatusMap = new Dictionary<FeeHeadStatus, string>
    {
        [FeeHeadStatus.Active]   = "active",
        [FeeHeadStatus.Inactive] = "inactive",
    };

    public static readonly IReadOnlyDictionary<FeeStructureStatus, string> FeeStructureStatusMap = new Dictionary<FeeStructureStatus, string>
    {
        [FeeStructureStatus.Draft]    = "draft",
        [FeeStructureStatus.Active]   = "active",
        [FeeStructureStatus.Archived] = "archived",
    };

    public static readonly IReadOnlyDictionary<FeeFrequency, string> FeeFrequencyMap = new Dictionary<FeeFrequency, string>
    {
        [FeeFrequency.OneTime]    = "one_time",
        [FeeFrequency.Monthly]    = "monthly",
        [FeeFrequency.Quarterly]  = "quarterly",
        [FeeFrequency.HalfYearly] = "half_yearly",
        [FeeFrequency.Annual]     = "annual",
    };

    public static readonly IReadOnlyDictionary<ProrationMethod, string> ProrationMethodMap = new Dictionary<ProrationMethod, string>
    {
        [ProrationMethod.FullMonth]    = "full_month",
        [ProrationMethod.CalendarDays] = "calendar_days",
    };

    public static readonly IReadOnlyDictionary<PaymentAllocationPriority, string> PaymentAllocationPriorityMap = new Dictionary<PaymentAllocationPriority, string>
    {
        [PaymentAllocationPriority.ManualOnly]  = "manual_only",
        [PaymentAllocationPriority.OldestFirst] = "oldest_first",
    };

    public static readonly IReadOnlyDictionary<LateFeeType, string> LateFeeTypeMap = new Dictionary<LateFeeType, string>
    {
        [LateFeeType.StagnantFixed] = "stagnant_fixed",
        [LateFeeType.Graduated]     = "graduated",
        [LateFeeType.Monthly]       = "monthly",
    };

    public static readonly IReadOnlyDictionary<InvoiceStatus, string> InvoiceStatusMap = new Dictionary<InvoiceStatus, string>
    {
        [InvoiceStatus.Draft]         = "draft",
        [InvoiceStatus.Sent]          = "sent",
        [InvoiceStatus.PartiallyPaid] = "partially_paid",
        [InvoiceStatus.Paid]          = "paid",
        [InvoiceStatus.Overdue]       = "overdue",
        [InvoiceStatus.Cancelled]     = "cancelled",
    };

    public static readonly IReadOnlyDictionary<PaymentStatus, string> PaymentStatusMap = new Dictionary<PaymentStatus, string>
    {
        [PaymentStatus.Completed] = "completed",
        [PaymentStatus.Pending]   = "pending",
        [PaymentStatus.Failed]    = "failed",
        [PaymentStatus.Bounced]   = "bounced",
        [PaymentStatus.Refunded]  = "refunded",
    };

    public static readonly IReadOnlyDictionary<PaymentMode, string> PaymentModeMap = new Dictionary<PaymentMode, string>
    {
        [PaymentMode.Cash]         = "cash",
        [PaymentMode.Cheque]       = "cheque",
        [PaymentMode.BankTransfer] = "bank_transfer",
        [PaymentMode.Online]       = "online",
        [PaymentMode.Card]         = "card",
    };

    public static readonly IReadOnlyDictionary<PaymentSource, string> PaymentSourceMap = new Dictionary<PaymentSource, string>
    {
        [PaymentSource.Counter]       = "counter",
        [PaymentSource.OnlineGateway] = "online_gateway",
        [PaymentSource.BulkCsv]       = "bulk_csv",
    };

    public static readonly IReadOnlyDictionary<CreditTransactionType, string> CreditTransactionTypeMap = new Dictionary<CreditTransactionType, string>
    {
        [CreditTransactionType.AdvancePayment]   = "advance_payment",
        [CreditTransactionType.AutoAdjustment]   = "auto_adjustment",
        [CreditTransactionType.ManualAdjustment] = "manual_adjustment",
        [CreditTransactionType.Refund]           = "refund",
        [CreditTransactionType.Expired]          = "expired",
    };

    public static readonly IReadOnlyDictionary<ConcessionType, string> ConcessionTypeMap = new Dictionary<ConcessionType, string>
    {
        [ConcessionType.Percentage] = "percentage",
        [ConcessionType.Fixed]      = "fixed",
        [ConcessionType.Sibling]    = "sibling",
        [ConcessionType.Merit]      = "merit",
    };

    public static readonly IReadOnlyDictionary<ConcessionAllocationStatus, string> ConcessionAllocationStatusMap = new Dictionary<ConcessionAllocationStatus, string>
    {
        [ConcessionAllocationStatus.Pending]  = "pending",
        [ConcessionAllocationStatus.Approved] = "approved",
        [ConcessionAllocationStatus.Rejected] = "rejected",
    };

    public static readonly IReadOnlyDictionary<RefundStatus, string> RefundStatusMap = new Dictionary<RefundStatus, string>
    {
        [RefundStatus.Pending]   = "pending",
        [RefundStatus.Approved]  = "approved",
        [RefundStatus.Completed] = "completed",
        [RefundStatus.Rejected]  = "rejected",
    };

    public static readonly IReadOnlyDictionary<RefundReason, string> RefundReasonMap = new Dictionary<RefundReason, string>
    {
        [RefundReason.Withdrawal]   = "withdrawal",
        [RefundReason.Overpayment]  = "overpayment",
        [RefundReason.Cancellation] = "cancellation",
        [RefundReason.Other]        = "other",
    };

    public static readonly IReadOnlyDictionary<FinancialAuditAction, string> FinancialAuditActionMap = new Dictionary<FinancialAuditAction, string>
    {
        [FinancialAuditAction.Create]  = "create",
        [FinancialAuditAction.Update]  = "update",
        [FinancialAuditAction.Delete]  = "delete",
        [FinancialAuditAction.Cancel]  = "cancel",
        [FinancialAuditAction.Restore] = "restore",
        [FinancialAuditAction.Reverse] = "reverse",
        [FinancialAuditAction.Waive]   = "waive",
        [FinancialAuditAction.Adjust]  = "adjust",
    };
}
