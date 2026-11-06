
using SchoolERP.Domain.Fees.Enums;

namespace SchoolERP.Application.Features.Fees.Builders;

/// <summary>
/// Determines how many times a FeeStructureLine should be charged
/// within a particular invoice billing period.
///
/// This class contains business rules only.
/// It does not access the database, create invoice entities,
/// calculate concessions, or save anything.
/// </summary>
public static class FeeHeadFrequencyPolicy
{
    /// <summary>
    /// Returns the number of fee occurrences covered by the given
    /// invoice billing period.
    ///
    /// All recurring fee frequencies are anchored to the beginning
    /// of the Academic Year.
    ///
    /// Examples:
    /// - Monthly fee in a Quarterly invoice    = 3
    /// - Quarterly fee in an Annual invoice    = 4
    /// - Half-Yearly fee in an Annual invoice  = 2
    /// - Annual fee in an Annual invoice       = 1
    /// - OneTime fee in a recurring invoice    = 0
    /// </summary>
    public static int GetOccurrenceCount(
        FeeFrequency feeFrequency,
        DateOnly academicYearStartDate,
        DateOnly academicYearEndDate,
        BillingPeriod billingPeriod)
    {
        // Block 1: Validate the supplied billing period.
        // An invalid date range must never produce a fee calculation.
        ArgumentNullException.ThrowIfNull(billingPeriod);

        if (academicYearEndDate < academicYearStartDate)
        {
            throw new ArgumentException(
                "Academic Year end date cannot be earlier than start date.",
                nameof(academicYearEndDate));
        }

        if (billingPeriod.EndDate < billingPeriod.StartDate)
        {
            throw new ArgumentException(
                "Billing period end date cannot be earlier than start date.",
                nameof(billingPeriod));
        }

        // Block 2: Ensure that the requested billing period
        // belongs to the supplied Academic Year.
        if (billingPeriod.StartDate < academicYearStartDate ||
            billingPeriod.EndDate > academicYearEndDate)
        {
            throw new ArgumentOutOfRangeException(
                nameof(billingPeriod),
                "Billing period must fall within the Academic Year.");
        }

        // Block 3: OneTime charges are excluded from recurring invoices.
        // Admission or other one-time charges will be handled separately.
        if (feeFrequency == FeeFrequency.OneTime)
        {
            return 0;
        }

        // Block 4: Determine the recurrence interval in months.
        // For example, Quarterly means one occurrence every 3 months.
        var intervalInMonths = GetIntervalInMonths(feeFrequency);

        var occurrenceCount = 0;

        // Block 5: Generate recurrence dates from the original
        // Academic Year start date.
        //
        // Important:
        // Every occurrence is calculated from the original anchor date,
        // rather than adding months to the previous occurrence.
        // This prevents cumulative date drift.
        for (var occurrenceIndex = 0; ; occurrenceIndex++)
        {
            var monthOffset = occurrenceIndex * intervalInMonths;

            var occurrenceDate = academicYearStartDate.AddMonths(
                monthOffset);

            // Stop when the next fee occurrence falls outside
            // the Academic Year.
            if (occurrenceDate > academicYearEndDate)
            {
                break;
            }

            // Count only those fee occurrences that fall within
            // the current invoice's date range.
            if (occurrenceDate >= billingPeriod.StartDate &&
                occurrenceDate <= billingPeriod.EndDate)
            {
                occurrenceCount++;
            }

            // Move to the next recurrence interval.
            // The next date is always calculated from AY start.
        }

        // Block 6: Return the number of charge occurrences.
        // The calling builder will multiply the configured fee amount
        // by this count.
        return occurrenceCount;
    }

    /// <summary>
    /// Maps each recurring FeeHead frequency to its recurrence interval.
    /// OneTime is intentionally excluded because it is not recurring.
    /// </summary>
    private static int GetIntervalInMonths(FeeFrequency frequency)
    {
        return frequency switch
        {
            // One charge occurrence every month.
            FeeFrequency.Monthly => 1,

            // One charge occurrence every three months.
            FeeFrequency.Quarterly => 3,

            // One charge occurrence every six months.
            FeeFrequency.HalfYearly => 6,

            // One charge occurrence every twelve months.
            FeeFrequency.Annual => 12,

            // Unknown values must fail explicitly instead of
            // silently producing an incorrect invoice.
            _ => throw new ArgumentOutOfRangeException(
                nameof(frequency),
                frequency,
                "Unsupported FeeHead frequency.")
        };
    }
}
