using SchoolERP.Domain.Fees.Enums;

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
}
