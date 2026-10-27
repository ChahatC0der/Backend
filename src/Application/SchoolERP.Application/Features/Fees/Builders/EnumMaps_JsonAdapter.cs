using SchoolERP.Domain.Fees.Enums;
using SchoolERP.Domain.Transport.Enums;

namespace SchoolERP.Application.Features.Fees.Builders;

/// <summary>
/// Application-layer copies of the DB string maps.
/// MUST stay identical to Infrastructure.Persistence.Converters.EnumMaps.
/// Fields carry the "Map" suffix so they never shadow the enum type name.
/// </summary>
public static class EnumMaps_JsonAdapter
{
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

    public static readonly IReadOnlyDictionary<FeeStructureStatus, string> FeeStructureStatusMap = new Dictionary<FeeStructureStatus, string>
    {
        [FeeStructureStatus.Draft]    = "draft",
        [FeeStructureStatus.Active]   = "active",
        [FeeStructureStatus.Archived] = "archived",
    };

    public static readonly IReadOnlyDictionary<ConcessionType, string> ConcessionTypeMap = new Dictionary<ConcessionType, string>
    {
        [ConcessionType.Percentage] = "percentage",
        [ConcessionType.Fixed]      = "fixed",
        [ConcessionType.Sibling]    = "sibling",
        [ConcessionType.Merit]      = "merit",
    };

    public static readonly IReadOnlyDictionary<PaymentMode, string> PaymentModeMap = new Dictionary<PaymentMode, string>
    {
        [PaymentMode.Cash]         = "cash",
        [PaymentMode.Cheque]       = "cheque",
        [PaymentMode.BankTransfer] = "bank_transfer",
        [PaymentMode.Online]       = "online",
        [PaymentMode.Card]         = "card",
    };

    public static readonly IReadOnlyDictionary<PaymentStatus, string> PaymentStatusMap = new Dictionary<PaymentStatus, string>
    {
        [PaymentStatus.Completed] = "completed",
        [PaymentStatus.Pending]   = "pending",
        [PaymentStatus.Failed]    = "failed",
        [PaymentStatus.Bounced]   = "bounced",
        [PaymentStatus.Refunded]  = "refunded",
    };

    public static readonly IReadOnlyDictionary<RefundReason, string> RefundReasonMap = new Dictionary<RefundReason, string>
    {
        [RefundReason.Withdrawal]   = "withdrawal",
        [RefundReason.Overpayment]  = "overpayment",
        [RefundReason.Cancellation] = "cancellation",
        [RefundReason.Other]        = "other",
    };

    public static readonly IReadOnlyDictionary<RefundStatus, string> RefundStatusMap = new Dictionary<RefundStatus, string>
    {
        [RefundStatus.Pending]   = "pending",
        [RefundStatus.Approved]  = "approved",
        [RefundStatus.Completed] = "completed",
        [RefundStatus.Rejected]  = "rejected",
    };

    // ===== Transport module (Phase 0) =====
    public static readonly IReadOnlyDictionary<VehicleType, string> VehicleTypeMap = new Dictionary<VehicleType, string>
    {
        [VehicleType.Bus]     = "bus",
        [VehicleType.Van]     = "van",
        [VehicleType.Minibus] = "minibus",
        [VehicleType.Car]     = "car",
        [VehicleType.Other]   = "other",
    };

    public static readonly IReadOnlyDictionary<VehicleStatus, string> VehicleStatusMap = new Dictionary<VehicleStatus, string>
    {
        [VehicleStatus.Active]      = "active",
        [VehicleStatus.Maintenance] = "maintenance",
        [VehicleStatus.Retired]     = "retired",
    };

    public static readonly IReadOnlyDictionary<VehicleDocumentType, string> VehicleDocumentTypeMap = new Dictionary<VehicleDocumentType, string>
    {
        [VehicleDocumentType.Insurance]    = "insurance",
        [VehicleDocumentType.Puc]          = "puc",
        [VehicleDocumentType.Fitness]      = "fitness",
        [VehicleDocumentType.Registration] = "registration",
    };

    public static readonly IReadOnlyDictionary<RouteStatus, string> RouteStatusMap = new Dictionary<RouteStatus, string>
    {
        [RouteStatus.Active]   = "active",
        [RouteStatus.Inactive] = "inactive",
    };

    public static readonly IReadOnlyDictionary<StopType, string> StopTypeMap = new Dictionary<StopType, string>
    {
        [StopType.PickupOnly] = "pickup_only",
        [StopType.DropOnly]   = "drop_only",
        [StopType.Both]       = "both",
    };

    public static readonly IReadOnlyDictionary<TransportFeeFrequency, string> TransportFeeFrequencyMap = new Dictionary<TransportFeeFrequency, string>
    {
        [TransportFeeFrequency.Monthly]    = "monthly",
        [TransportFeeFrequency.Quarterly]  = "quarterly",
        [TransportFeeFrequency.HalfYearly] = "half_yearly",
    };
}
