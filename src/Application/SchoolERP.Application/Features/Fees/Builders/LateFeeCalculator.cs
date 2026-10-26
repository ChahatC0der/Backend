using System.Text.Json;
using SchoolERP.Domain.Fees.Enums;
using SchoolERP.Domain.Fees.ValueObjects;

namespace SchoolERP.Application.Features.Fees.Builders;

/// <summary>
/// Pure calculator for late fee PER SOURCE LINE. Called from RecalculateLateFeesCommandHandler.
/// Returns 0 when not yet past DueDate + GraceDays.
/// Cap is applied against LateFeeCapBase (frozen at generation) — NOT against NetPayable.
/// </summary>
public static class LateFeeCalculator
{
    public sealed record RuleSnapshot(
        string? TypeSnapshot,
        decimal? ValueSnapshot,
        int? GraceDaysSnapshot,
        decimal? CapPercentSnapshot,
        string? SlabConfigSnapshot,
        decimal CapBase);

    /// <summary>
    /// Returns the late fee for one source line as of a given date.
    /// Returns 0 if not yet past grace period, cap is 0, or rule is unset.
    /// </summary>
    public static decimal Calculate(RuleSnapshot rule, DateTime dueDate, DateTime asOf)
    {
        if (string.IsNullOrWhiteSpace(rule.TypeSnapshot)) return 0m;
        if (rule.CapBase <= 0) return 0m;

        // daysAfterGrace: how many days past (dueDate + graceDays)
        var daysPastDue = (asOf.Date - dueDate.Date).TotalDays;
        if (daysPastDue <= 0) return 0m;

        var graceDays = rule.GraceDaysSnapshot ?? 0;
        var daysAfterGrace = daysPastDue - graceDays;
        if (daysAfterGrace <= 0) return 0m;

        if (!TryParseType(rule.TypeSnapshot, out var type)) return 0m;

        var calculated = type switch
        {
            LateFeeType.StagnantFixed => rule.ValueSnapshot ?? 0m,
            LateFeeType.Monthly       => MonthlyAmount(rule.ValueSnapshot ?? 0m, dueDate, asOf),
            LateFeeType.Graduated     => GraduatedAmount(rule.SlabConfigSnapshot, (int)daysAfterGrace),
            _                         => 0m,
        };

        if (calculated <= 0) return 0m;

        var capPercent = rule.CapPercentSnapshot ?? 0m;
        if (capPercent <= 0) return Money.Round(calculated);

        var capAmount = Money.Round(rule.CapBase * capPercent / 100m);
        return Money.Round(Money.CapAt(calculated, capAmount));
    }

    private static decimal MonthlyAmount(decimal perMonth, DateTime dueDate, DateTime asOf)
    {
        // Number of full months elapsed since due date
        var months = ((asOf.Year - dueDate.Year) * 12) + asOf.Month - dueDate.Month;
        if (asOf.Day < dueDate.Day) months -= 1;
        if (months <= 0) return 0m;
        return perMonth * months;
    }

    private static decimal GraduatedAmount(string? slabJson, int daysAfterGrace)
    {
        if (string.IsNullOrWhiteSpace(slabJson)) return 0m;

        List<LateFeeSlabConfig>? slabs;
        try { slabs = JsonSerializer.Deserialize<List<LateFeeSlabConfig>>(slabJson); }
        catch { return 0m; }
        if (slabs is null || slabs.Count == 0) return 0m;

        foreach (var slab in slabs)
        {
            if (daysAfterGrace >= slab.FromDay && daysAfterGrace <= slab.ToDay)
                return slab.Amount;
        }
        return 0m;
    }

    private static bool TryParseType(string value, out LateFeeType type)
    {
        foreach (var kv in EnumMaps_JsonAdapter.LateFeeTypeMap)
            if (string.Equals(kv.Value, value, StringComparison.OrdinalIgnoreCase)) { type = kv.Key; return true; }
        type = default; return false;
    }
}
