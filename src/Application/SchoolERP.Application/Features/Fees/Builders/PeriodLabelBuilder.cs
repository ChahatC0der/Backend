namespace SchoolERP.Application.Features.Fees.Builders;

/// <summary>
/// Builds PeriodLabel values — the discriminator used by UQ_Invoices_StudentYearPeriod.
/// Rules:
///   BulkGenerate       -> caller supplies (e.g. "Apr 2025", "Q1 (Apr-Jun)")
///   OptInOptional      -> "OPT-{FeeHeadCode}"   (stable — one live invoice per student/AY/head)
///   Manual             -> "MAN-{Guid[..8]}"     (unique per request)
///   Regenerate         -> copy source PeriodLabel after cancelling the old invoice
/// </summary>
public static class PeriodLabelBuilder
{
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
