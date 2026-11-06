using System.Globalization;
using SchoolERP.Domain.Fees.Enums;
// BillingPeriod ka using bhi rakhna — same jo BillingPeriodCalculator.cs me laga hai

namespace SchoolERP.Application.Features.Fees.Builders;

/// <summary>
/// Builds PeriodLabel values — the discriminator used by UQ_Invoices_StudentYearPeriod.
/// Rules:
///   Periodic (sequence-based) -> derived from BillingPeriod
///                                ("Apr 2026", "Q1 (Apr-Jun)", "H1 (Apr-Sep)",
///                                 "Annual (Apr 2026 - Mar 2027)")
///   BulkGenerate       -> caller supplies (e.g. "Apr 2025", "Q1 (Apr-Jun)")
///   OptInOptional      -> "OPT-{FeeHeadCode}"   (stable — one live invoice per student/AY/head)
///   Manual             -> "MAN-{Guid[..8]}"     (unique per request)
///   Regenerate         -> copy source PeriodLabel after cancelling the old invoice
///
/// Periodic labels are DETERMINISTIC (same period -> same label forever)
/// and culture-invariant, because they are persisted and participate in the
/// uniqueness index.
/// </summary>
public static class PeriodLabelBuilder
{
    /// <summary>
    /// Derives the PeriodLabel for a sequence-based periodic invoice from the
    /// resolved BillingPeriod (BillingPeriodCalculator output). The request
    /// DTO no longer supplies the label.
    /// </summary>
    public static string ForBillingPeriod(BillingPeriod billingPeriod)
    {
        ArgumentNullException.ThrowIfNull(billingPeriod);

        // InvariantCulture: labels are stored in DB and used in
        // UQ_Invoices_StudentYearPeriod — server locale (hindi OS, etc.) se
        // kabhi change nahi hone chahiye.
        var startMonth = billingPeriod.StartDate.ToString("MMM", CultureInfo.InvariantCulture);
        var endMonth = billingPeriod.EndDate.ToString("MMM", CultureInfo.InvariantCulture);

        return billingPeriod.Frequency switch
        {
            InvoiceGenerationFrequency.Monthly =>
                $"{startMonth} {billingPeriod.StartDate:yyyy}",                          // "Apr 2026"

            InvoiceGenerationFrequency.Quarterly =>
                $"Q{billingPeriod.SequenceNumber} ({startMonth}-{endMonth})",            // "Q1 (Apr-Jun)"

            InvoiceGenerationFrequency.HalfYearly =>
                $"H{billingPeriod.SequenceNumber} ({startMonth}-{endMonth})",            // "H1 (Apr-Sep)"

            InvoiceGenerationFrequency.Annual =>
                $"Annual ({startMonth} {billingPeriod.StartDate:yyyy} - {endMonth} {billingPeriod.EndDate:yyyy})",   // "Annual (Apr 2026 - Mar 2027)"

            _ => throw new ArgumentOutOfRangeException(
                nameof(billingPeriod),
                billingPeriod.Frequency,
                "Unsupported invoice generation frequency.")
        };
    }

    public static string ForBulk(string rawLabel)
    {
        if (string.IsNullOrWhiteSpace(rawLabel))
            throw new ArgumentException("PeriodLabel is required for bulk generation.", nameof(rawLabel));
        return rawLabel.Trim();
    }

    public static string ForOptIn(string feeHeadCode)
    {
        if (string.IsNullOrWhiteSpace(feeHeadCode))
            throw new ArgumentException("FeeHeadCode is required for opt-in.", nameof(feeHeadCode));
        return $"OPT-{feeHeadCode.Trim().ToUpperInvariant()}";
    }

    public static string ForManual()
        => $"MAN-{Guid.NewGuid():N}"[..12];
}