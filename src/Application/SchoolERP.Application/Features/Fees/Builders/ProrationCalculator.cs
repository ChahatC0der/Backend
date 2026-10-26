using SchoolERP.Domain.Fees.Enums;
using SchoolERP.Domain.Fees.ValueObjects;

namespace SchoolERP.Application.Features.Fees.Builders;

/// <summary>
/// Prorates a fee amount for a billing period, given the fee structure's ApplicableFrom/To.
/// Modes:
///   FullMonth    -> any partial coverage counts as full amount (no reduction).
///   CalendarDays -> (overlap days / period days) * amount.
/// If period fully within ApplicableFrom/To -> full amount (no proration).
/// </summary>
public static class ProrationCalculator
{
    public static decimal Prorate(
        decimal fullAmount,
        ProrationMethod method,
        DateOnly periodStart,
        DateOnly periodEnd,
        DateOnly applicableFrom,
        DateOnly applicableTo)
    {
        var periodDays = (periodEnd.DayNumber - periodStart.DayNumber) + 1;
        if (periodDays <= 0) return 0m;

        // No overlap at all
        if (periodEnd < applicableFrom || periodStart > applicableTo) return 0m;

        // Full overlap -> no proration
        if (periodStart >= applicableFrom && periodEnd <= applicableTo) return fullAmount;

        // Partial overlap
        if (method == ProrationMethod.FullMonth) return fullAmount;

        var overlapStart = periodStart > applicableFrom ? periodStart : applicableFrom;
        var overlapEnd   = periodEnd   < applicableTo   ? periodEnd   : applicableTo;
        var overlapDays = (overlapEnd.DayNumber - overlapStart.DayNumber) + 1;
        if (overlapDays <= 0) return 0m;

        return Money.Round(fullAmount * overlapDays / (decimal)periodDays);
    }
}
