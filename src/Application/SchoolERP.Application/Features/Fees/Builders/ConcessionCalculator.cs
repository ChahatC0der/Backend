using SchoolERP.Domain.Fees.Enums;
using SchoolERP.Domain.Fees.ValueObjects;
using ConcessionEntity = SchoolERP.Domain.Fees.Entities.Concession;

namespace SchoolERP.Application.Features.Fees.Builders;

/// <summary>
/// Pure stacking engine — spec §7:
///   1. Percentage-style (CalculationType.Percentage) applied first
///      (Value is % of lineAmount).
///   2. Fixed-style (CalculationType.Fixed) applied second
///      (Value is straight amount).
///   3. Cap total at lineAmount so NetPayable never goes negative.
///
/// Caller responsibility (InvoiceBuilder):
///   - decide WHICH concessions apply to a given line
///     (head-specific ones for that head + total-level ones
///      whose FeeHeadId == null)
///   - load only live concessions (DeletedAt == null, date-valid,
///     tenant/branch scoped)
///   - load only Approved allocations
///
/// This method does NOT filter by FeeHeadId. It trusts the caller's
/// list. Total-level concessions arrive here as just another item in
/// `applicable`; their percentage is taken against this line's
/// lineAmount — which is exactly what spec §7 (additive on original
/// gross, not on remaining) means.
/// </summary>
public static class ConcessionCalculator
{
    public static decimal Calculate(decimal lineAmount, IEnumerable<ConcessionEntity> applicable)
    {
        if (lineAmount <= 0m || applicable is null) return 0m;

        decimal percentageSum = 0m;
        decimal fixedSum = 0m;

        foreach (var c in applicable)
        {
            switch (c.CalculationType)
            {
                case ConcessionCalculationType.Percentage:
                    // Value is interpreted as percent (e.g., 50 → 50%).
                    // Each percentage concession is computed against the
                    // SAME lineAmount (additive stacking, not compounding).
                    percentageSum += lineAmount * c.Value / 100m;
                    break;

                case ConcessionCalculationType.Fixed:
                    // Value is a flat rupee amount, applied after percentages.
                    fixedSum += c.Value;
                    break;
            }
        }

        var total = Money.Round(percentageSum + fixedSum);
        return Money.CapAt(total, lineAmount);
    }
}