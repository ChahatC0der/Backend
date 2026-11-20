using System.Text.Json;
using SchoolERP.Domain.Fees.Enums;
using SchoolERP.Domain.Fees.ValueObjects;

namespace SchoolERP.Application.Features.Fees.Builders;

/// <summary>
/// LAYER 1 — ATOM. Single rule, single line, single calc. No I/O, no clock.
/// Engine calls this; engine never does math.
///
/// All method bodies are isolated so a future "settings module" can swap
/// any one strategy (e.g., replace MonthlyAmount with 30-day fixed) by
/// editing one method, without touching the others.
/// </summary>
public static class LateFeeCalculator
{
    // ────────────── CONTRACTS ──────────────

    /// <summary>
    /// Frozen snapshot of late-fee rules for one source line (or one invoice).
    /// Caller (engine) builds this from snapshot columns.
    /// </summary>
    public sealed record RuleSnapshot(
        string? TypeSnapshot,           // 'stagnant_fixed' | 'graduated' | 'monthly'
        decimal? ValueSnapshot,         // per-day, per-month, or base (depending on type)
        int? GraceDaysSnapshot,
        decimal? CapPercentSnapshot,    // null/0 = no cap
        string? SlabConfigSnapshot,     // JSON, graduated only
        decimal CapBase);               // line.NetAmount or invoice.NetPayable (frozen)

    /// <summary>
    /// Calc result for one rule application.
    /// Engine uses all 3 fields for breakdown display + invoice update.
    /// </summary>
    public sealed record Result(
        int ChargeableDays,    // informational: how many days past grace
        decimal RawAmount,     // before cap
        decimal CappedAmount)  // after cap (= final)
    {
        public decimal Final => CappedAmount;
    }

    // ────────────── ENTRY POINT ──────────────

    /// <summary>
    /// Returns chargeable days + raw + capped amounts for one rule.
    /// All early-exit cases return Zero (no charge).
    /// </summary>
    public static Result Calculate(RuleSnapshot rule, DateTime dueDate, DateTime asOf)
    {
        // Guards: rule not configured or cap base invalid
        if (string.IsNullOrWhiteSpace(rule.TypeSnapshot)) return Zero();
        if (rule.CapBase <= 0m) return Zero();

        // Days past due + grace
        var chargeableDays = CalculateChargeableDays(dueDate, rule.GraceDaysSnapshot, asOf);
        if (chargeableDays <= 0) return Zero();

        // Parse rule type
        if (!TryParseType(rule.TypeSnapshot, out var type)) return Zero();

        // Method dispatch — each method is its own function (modular)
        var raw = type switch
        {
            LateFeeType.StagnantFixed => StagnantFixedAmount(rule.ValueSnapshot ?? 0m, chargeableDays),
            LateFeeType.Monthly => MonthlyAmount(rule.ValueSnapshot ?? 0m, dueDate, asOf),
            LateFeeType.Graduated => GraduatedAmount(rule.SlabConfigSnapshot, chargeableDays),
            _ => 0m,
        };

        if (raw <= 0m) return Zero();

        // Apply cap (null/zero cap = no cap)
        var capped = ApplyCap(raw, rule.CapPercentSnapshot, rule.CapBase);

        return new Result(chargeableDays, Money.Round(raw), Money.Round(capped));
    }

    // ────────────── MODULAR HELPERS (one job each) ──────────────

    /// <summary>
    /// Days past dueDate + grace. Returns 0 if not yet overdue or still in grace.
    /// PUBLIC so engine can call directly when only days are needed for display,
    /// without running full calculation.
    /// </summary>
    public static int CalculateChargeableDays(DateTime dueDate, int? graceDays, DateTime asOf)
    {
        var daysPastDue = (int)(asOf.Date - dueDate.Date).TotalDays;
        if (daysPastDue <= 0) return 0;

        var grace = graceDays ?? 0;
        var chargeable = daysPastDue - grace;
        return chargeable > 0 ? chargeable : 0;
    }

    /// <summary>
    /// Stagnant fixed: value × chargeable days.
    /// e.g., ₹20/day × 5 days = ₹100.
    /// </summary>
    private static decimal StagnantFixedAmount(decimal perDay, int chargeableDays)
        => perDay * chargeableDays;

    /// <summary>
    /// Monthly: value per FULL calendar month elapsed since dueDate.
    /// Calendar months (NOT 30-day fixed) — confirmed by spec owner.
    ///
    /// Logic: count month boundaries crossed since dueDate.
    /// If asOf.Day &lt; dueDate.Day, current month not yet complete → don't count it.
    /// e.g., dueDate=Jan 10, asOf=Mar 5 → 2 month boundaries, but Mar not complete → 1 month.
    ///
    /// To swap to "30-day fixed + >15 partial rule" in future, replace this
    /// single method — no other code needs to change.
    /// </summary>
    private static decimal MonthlyAmount(decimal perMonth, DateTime dueDate, DateTime asOf)
    {
        var months = ((asOf.Year - dueDate.Year) * 12) + asOf.Month - dueDate.Month;
        if (asOf.Day < dueDate.Day) months -= 1;   // current month not yet complete
        if (months <= 0) return 0m;
        return perMonth * months;
    }

    /// <summary>
    /// Graduated: range-based flat slabs.
    /// JSON shape (camelCase, validated by DB ISJSON + DTO validator):
    ///   [{"fromDay":1,"toDay":10,"amount":50}, {"fromDay":11,"toDay":20,"amount":100}]
    ///
    /// Returns slab.Amount where chargeableDays falls in [FromDay, ToDay].
    /// Past last slab's ToDay → escalates to last slab's amount
    /// (alternative policy: return 0; escalate is more common in ERPs).
    /// </summary>
    private static decimal GraduatedAmount(string? slabJson, int chargeableDays)
    {
        if (string.IsNullOrWhiteSpace(slabJson)) return 0m;

        List<LateFeeSlabConfig>? slabs;
        try
        {
            slabs = JsonSerializer.Deserialize<List<LateFeeSlabConfig>>(slabJson, SlabJsonOptions);
        }
        catch
        {
            return 0m;   // invalid JSON → no charge (validator should catch earlier, this is defense)
        }

        if (slabs is null || slabs.Count == 0) return 0m;

        foreach (var slab in slabs)
        {
            if (chargeableDays >= slab.FromDay && chargeableDays <= slab.ToDay)
                return slab.Amount;
        }

        // Past last slab → use last amount (escalation policy)
        return slabs[^1].Amount;
    }

    /// <summary>
    /// Cap: min(raw, capPercent% × capBase).
    /// Null or zero cap → no cap (return raw as-is).
    /// </summary>
    private static decimal ApplyCap(decimal raw, decimal? capPercent, decimal capBase)
    {
        var cap = capPercent ?? 0m;
        if (cap <= 0m) return raw;

        var capAmount = capBase * cap / 100m;
        return Money.CapAt(raw, capAmount);
    }

    // ────────────── PRIVATE UTILITIES ──────────────

    private static Result Zero() => new(0, 0m, 0m);

    private static bool TryParseType(string value, out LateFeeType type)
    {
        foreach (var kv in EnumMaps_JsonAdapter.LateFeeTypeMap)
        {
            if (string.Equals(kv.Value, value, StringComparison.OrdinalIgnoreCase))
            {
                type = kv.Key;
                return true;
            }
        }
        type = default;
        return false;
    }

    // CamelCase JSON → PascalCase C# properties (DB stores camelCase per LateFeeSlabConfig example)
    private static readonly JsonSerializerOptions SlabJsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };
}