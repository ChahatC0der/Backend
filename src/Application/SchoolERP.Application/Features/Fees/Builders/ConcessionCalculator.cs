using SchoolERP.Domain.Fees.Enums;
using SchoolERP.Domain.Fees.ValueObjects;
using ConcessionEntity = SchoolERP.Domain.Fees.Entities.Concession;

namespace SchoolERP.Application.Features.Fees.Builders;

/// <summary>
/// Pure stacking engine — spec §7:
///   1. Percentage-style (Percentage, Sibling, Merit) applied first (Value is % of lineAmount).
///   2. Fixed-style (Fixed) applied second (Value is straight amount).
///   3. Cap total at lineAmount so NetPayable never goes negative.
/// All money math via Money.Round.
/// </summary>
public static class ConcessionCalculator
{
    public static decimal Calculate(decimal lineAmount, IEnumerable<ConcessionEntity> applicable)
    {
        if (lineAmount <= 0 || applicable is null) return 0m;

        decimal percentageSum = 0m;
        decimal fixedSum = 0m;

        foreach (var c in applicable)
        {
            switch (c.Type)
            {
                case ConcessionType.Percentage:
                case ConcessionType.Sibling:
                case ConcessionType.Merit:
                    percentageSum += lineAmount * c.Value / 100m;
                    break;
                case ConcessionType.Fixed:
                    fixedSum += c.Value;
                    break;
            }
        }

        var total = Money.Round(percentageSum + fixedSum);
        return Money.CapAt(total, lineAmount);
    }
}
