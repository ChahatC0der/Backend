using SchoolERP.Domain.Fees.Enums;

namespace SchoolERP.Application.Features.Fees.Builders;

/// <summary>
/// Calculates billing periods within an academic year.
///
/// Supported frequencies:
/// Monthly     : 1 month
/// Quarterly   : 3 months
/// HalfYearly  : 6 months
/// Annual      : 12 months
///
/// Period boundaries are calculated relative to the original
/// academic year start date to avoid cumulative date drift.
/// </summary>
public static class BillingPeriodCalculator
{
    public static IReadOnlyList<BillingPeriod> Calculate(
        DateOnly academicYearStartDate,
        DateOnly academicYearEndDate,
        InvoiceGenerationFrequency frequency)
    {
        if (academicYearEndDate < academicYearStartDate)
        {
            throw new ArgumentException(
                "Academic Year end date cannot be earlier than start date.",
                nameof(academicYearEndDate));
        }

        var monthsPerPeriod = GetMonthsPerPeriod(frequency);

        var periods = new List<BillingPeriod>();

        var sequenceNumber = 1;

        while (true)
        {
            var startMonthOffset = (sequenceNumber - 1) * monthsPerPeriod;
            var endMonthOffset = sequenceNumber * monthsPerPeriod;

            var periodStartDate =
                academicYearStartDate.AddMonths(startMonthOffset);

            if (periodStartDate > academicYearEndDate)
                break;

            var nextPeriodStartDate =
                academicYearStartDate.AddMonths(endMonthOffset);

            var calculatedEndDate =
                nextPeriodStartDate.AddDays(-1);

            var periodEndDate = calculatedEndDate <= academicYearEndDate
                ? calculatedEndDate
                : academicYearEndDate;

            periods.Add(new BillingPeriod(
                SequenceNumber: sequenceNumber,
                Frequency: frequency,
                StartDate: periodStartDate,
                EndDate: periodEndDate
            ));

            if (periodEndDate == academicYearEndDate)
                break;

            sequenceNumber++;
        }

        return periods.AsReadOnly();
    }

    private static int GetMonthsPerPeriod(
        InvoiceGenerationFrequency frequency)
    {
        return frequency switch
        {
            InvoiceGenerationFrequency.Monthly => 1,

            InvoiceGenerationFrequency.Quarterly => 3,

            InvoiceGenerationFrequency.HalfYearly => 6,

            InvoiceGenerationFrequency.Annual => 12,

            _ => throw new ArgumentOutOfRangeException(
                nameof(frequency),
                frequency,
                "Unsupported invoice generation frequency.")
        };
    }
}