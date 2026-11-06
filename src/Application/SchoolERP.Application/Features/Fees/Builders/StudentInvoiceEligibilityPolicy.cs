
namespace SchoolERP.Application.Features.Fees.Builders;

/// <summary>
/// Determines whether a student is eligible to receive an invoice
/// for a particular billing period.
///
/// Eligibility is determined using:
/// 1. Student enrollment dates.
/// 2. Student leaving date.
/// 3. Fee structure applicability dates.
///
/// This is a pure business policy:
/// - No database access.
/// - No invoice creation.
/// - No proration calculation.
/// - No financial mutations.
/// </summary>
public static class StudentInvoiceEligibilityPolicy
{
    /// <summary>
    /// Checks whether the student's enrollment and fee structure
    /// overlap with the requested billing period.
    ///
    /// All date comparisons are inclusive.
    ///
    /// If the student joins or leaves during a billing period,
    /// the student remains eligible for that period.
    ///
    /// Proration is intentionally not applied here.
    /// </summary>
    public static bool IsEligible(
        DateOnly enrolledAt,
        DateOnly? leftAt,
        DateTime structureApplicableFrom,
        DateTime structureApplicableTo,
        BillingPeriod billingPeriod)
    {
        // Block 1: Validate the billing period.
        // A period with an invalid date range cannot be evaluated.
        ArgumentNullException.ThrowIfNull(billingPeriod);

        if (billingPeriod.EndDate < billingPeriod.StartDate)
        {
            throw new ArgumentException(
                "Billing period end date cannot be earlier than start date.",
                nameof(billingPeriod));
        }

        // Block 2: Validate the student's enrollment dates.
        // A student cannot leave before their enrollment date.
        if (leftAt.HasValue && leftAt.Value < enrolledAt)
        {
            throw new ArgumentException(
                "Student leaving date cannot be earlier than enrollment date.",
                nameof(leftAt));
        }

        // Block 3: Convert FeeStructure DateTime values into DateOnly.
        // The structure applicability fields represent calendar dates,
        // so their time components are intentionally ignored.
        var structureStartDate =
            DateOnly.FromDateTime(structureApplicableFrom);

        var structureEndDate =
            DateOnly.FromDateTime(structureApplicableTo);

        // Block 4: Validate FeeStructure applicability range.
        if (structureEndDate < structureStartDate)
        {
            throw new ArgumentException(
                "Fee structure applicable-to date cannot be earlier than applicable-from date.",
                nameof(structureApplicableTo));
        }

        // Block 5: Determine the student's effective enrollment end date.
        // If LeftAt is null, the enrollment has no recorded end date.
        // DateOnly.MaxValue represents an open-ended enrollment.
        var enrollmentEndDate = leftAt ?? DateOnly.MaxValue;

        // Block 6: Check whether student enrollment overlaps
        // the requested billing period.
        //
        // Examples:
        // Joined before period starts -> Eligible.
        // Joined during the period   -> Eligible.
        // Joined after period ends   -> Not eligible.
        // Left before period starts  -> Not eligible.
        // Left during the period     -> Eligible.
        var enrollmentOverlapsPeriod = DatesOverlap(
            enrolledAt,
            enrollmentEndDate,
            billingPeriod.StartDate,
            billingPeriod.EndDate);

        if (!enrollmentOverlapsPeriod)
        {
            return false;
        }

        // Block 7: Check whether the FeeStructure is applicable
        // during at least part of the billing period.
        //
        // A structure that starts during the period or expires
        // during the period is considered applicable for that period.
        var structureOverlapsPeriod = DatesOverlap(
            structureStartDate,
            structureEndDate,
            billingPeriod.StartDate,
            billingPeriod.EndDate);

        if (!structureOverlapsPeriod)
        {
            return false;
        }

        // Block 8: Both enrollment and FeeStructure must overlap
        // the billing period for invoice eligibility.
        return true;
    }

    /// <summary>
    /// Checks whether two inclusive date ranges overlap.
    ///
    /// Two ranges overlap when:
    /// FirstStart <= SecondEnd
    /// AND
    /// FirstEnd >= SecondStart
    /// </summary>
    private static bool DatesOverlap(
        DateOnly firstStart,
        DateOnly firstEnd,
        DateOnly secondStart,
        DateOnly secondEnd)
    {
        // Return true when the two date ranges share
        // at least one calendar date.
        return firstStart <= secondEnd &&
               firstEnd >= secondStart;
    }
}
