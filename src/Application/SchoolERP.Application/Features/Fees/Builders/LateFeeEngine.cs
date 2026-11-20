using SchoolERP.Domain.Fees.ValueObjects;

namespace SchoolERP.Application.Features.Fees.Builders;

// ────────────── CONTRACTS (pure DTOs, no entity dependency) ──────────────

/// <summary>
/// One invoice line's late-fee-relevant snapshot.
/// Built by handler from InvoiceLine entity (persisted) — NOT from InvoiceLineDraft.
/// </summary>
public sealed record LateFeeLineInput(
    long LineId,
    decimal NetAmount,             // for breakdown display (cap base is separate field)
    bool LateFeeApplicable,        // per-head mode: skip if false
    bool IsLateFeeLine,            // always skip — no compounding late fee on late fee line
    string? LateFeeTypeSnapshot,
    decimal? LateFeeValueSnapshot,
    int? LateFeeGraceDaysSnapshot,
    decimal? LateFeeCapPercentSnapshot,
    string? LateFeeSlabConfigSnapshot,
    decimal LateFeeCapBase);       // line.NetAmount (frozen at generation)

/// <summary>
/// Whole invoice + its lines, ready for late-fee evaluation.
/// Mode dispatch uses LateFeeModeSnapshot; whole-invoice fields used only
/// when mode == "whole_invoice".
/// </summary>
public sealed record LateFeeInvoiceInput(
    DateTime DueDate,
    string? LateFeeModeSnapshot,                  // null = treat as 'per_head' (legacy)
                                                  // ── whole-invoice fields (mode == whole_invoice only) ──
    string? InvoiceLateFeeTypeSnapshot,
    decimal? InvoiceLateFeeValueSnapshot,
    int? InvoiceLateFeeGraceDaysSnapshot,
    decimal? InvoiceLateFeeCapPercentSnapshot,
    string? InvoiceLateFeeSlabConfigSnapshot,
    decimal? InvoiceLateFeeCapBase,              // = NetPayable (frozen)
                                                 // ── per-head mode: lines ──
    IReadOnlyList<LateFeeLineInput> Lines);

/// <summary>
/// Per-line breakdown — for audit / debug display.
/// Skipped lines have SkippedReason set; ChargeableDays/Raw/Capped all 0.
/// </summary>
public sealed record LateFeeLineBreakdown(
    long? LineId,            // null for whole-invoice mode (no specific line)
    int ChargeableDays,
    decimal RawAmount,
    decimal CappedAmount,
    string? SkippedReason);  // null = not skipped; "not_applicable" | "late_fee_line" | "no_rule"

/// <summary>
/// Engine result. Mode is echoed for client display.
/// TotalLateFee = sum of all CappedAmounts (already rounded per-line).
/// </summary>
public sealed record LateFeeResult(
    string Mode,
    decimal TotalLateFee,
    IReadOnlyList<LateFeeLineBreakdown> Breakdown);

// ────────────── ENGINE ──────────────

/// <summary>
/// LAYER 2 — invoice-level late-fee orchestrator. PURE: no I/O, no entities, no clock.
///
/// WHO (which invoice, which lines) caller decides; engine decides HOW MUCH.
/// Mirrors ConcessionEngine pattern: contract → phase 1 (dispatch) → phase 2 (loop/single) → combine.
///
/// Phase 2 CRON will call this same engine — no logic duplication.
/// </summary>
public static class LateFeeEngine
{
    public static LateFeeResult Evaluate(LateFeeInvoiceInput invoice, DateTime asOf)
    {
        var mode = NormalizeMode(invoice.LateFeeModeSnapshot);

        return mode switch
        {
            "none" => Zero("none"),
            "whole_invoice" => EvaluateWholeInvoice(invoice, asOf),
            _ => EvaluatePerHead(invoice, asOf)   // default + legacy
        };
    }

    // ────────────── MODE: PER HEAD ──────────────

    private static LateFeeResult EvaluatePerHead(LateFeeInvoiceInput invoice, DateTime asOf)
    {
        var breakdown = new List<LateFeeLineBreakdown>(invoice.Lines.Count);
        decimal total = 0m;

        foreach (var line in invoice.Lines)
        {
            // Skip 1: late fee line itself (no compounding)
            if (line.IsLateFeeLine)
            {
                breakdown.Add(Skipped(line.LineId, "late_fee_line"));
                continue;
            }

            // Skip 2: not applicable for this head
            if (!line.LateFeeApplicable)
            {
                breakdown.Add(Skipped(line.LineId, "not_applicable"));
                continue;
            }

            // Skip 3: rule was NULL at generation time (misconfigured head)
            if (string.IsNullOrWhiteSpace(line.LateFeeTypeSnapshot))
            {
                breakdown.Add(Skipped(line.LineId, "no_rule"));
                continue;
            }

            // Build rule snapshot from line + calculate
            var rule = new LateFeeCalculator.RuleSnapshot(
                line.LateFeeTypeSnapshot,
                line.LateFeeValueSnapshot,
                line.LateFeeGraceDaysSnapshot,
                line.LateFeeCapPercentSnapshot,
                line.LateFeeSlabConfigSnapshot,
                line.LateFeeCapBase);

            var calc = LateFeeCalculator.Calculate(rule, invoice.DueDate, asOf);

            breakdown.Add(new LateFeeLineBreakdown(
                line.LineId,
                calc.ChargeableDays,
                calc.RawAmount,
                calc.CappedAmount,
                SkippedReason: null));

            total += calc.CappedAmount;
        }

        return new LateFeeResult("per_head", Money.Round(total), breakdown);
    }

    // ────────────── MODE: WHOLE INVOICE ──────────────

    private static LateFeeResult EvaluateWholeInvoice(LateFeeInvoiceInput invoice, DateTime asOf)
    {
        // Skip: rule not configured at generation time
        if (string.IsNullOrWhiteSpace(invoice.InvoiceLateFeeTypeSnapshot))
            return Zero("whole_invoice");

        // Skip: cap base missing/invalid (shouldn't happen if builder froze correctly)
        var capBase = invoice.InvoiceLateFeeCapBase ?? 0m;
        if (capBase <= 0m)
            return Zero("whole_invoice");

        // Build rule snapshot from invoice-level fields + calculate
        var rule = new LateFeeCalculator.RuleSnapshot(
            invoice.InvoiceLateFeeTypeSnapshot,
            invoice.InvoiceLateFeeValueSnapshot,
            invoice.InvoiceLateFeeGraceDaysSnapshot,
            invoice.InvoiceLateFeeCapPercentSnapshot,
            invoice.InvoiceLateFeeSlabConfigSnapshot,
            capBase);

        var calc = LateFeeCalculator.Calculate(rule, invoice.DueDate, asOf);

        // Single breakdown entry (no specific line — invoice-level)
        var breakdown = new List<LateFeeLineBreakdown>
        {
            new(null, calc.ChargeableDays, calc.RawAmount, calc.CappedAmount, null)
        };

        return new LateFeeResult("whole_invoice", Money.Round(calc.CappedAmount), breakdown);
    }

    // ────────────── UTILITIES ──────────────

    /// <summary>
    /// Normalize mode string. Legacy (null) → "per_head" (line-level snapshots exist).
    /// Unknown values → "per_head" (safe fallback, matches existing data).
    /// </summary>
    private static string NormalizeMode(string? mode)
    {
        if (string.IsNullOrWhiteSpace(mode)) return "per_head";

        return mode.ToLowerInvariant() switch
        {
            "none" => "none",
            "per_head" => "per_head",
            "whole_invoice" => "whole_invoice",
            _ => "per_head"
        };
    }

    private static LateFeeResult Zero(string mode)
        => new(mode, 0m, Array.Empty<LateFeeLineBreakdown>());

    private static LateFeeLineBreakdown Skipped(long lineId, string reason)
        => new(lineId, 0, 0m, 0m, reason);
}