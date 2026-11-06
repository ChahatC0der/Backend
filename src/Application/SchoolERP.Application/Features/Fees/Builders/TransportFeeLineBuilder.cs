
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Domain.Fees.Enums;
using SchoolERP.Domain.Fees.ValueObjects;
using SchoolERP.Domain.Shared.Results;

using FeeHeadEntity = SchoolERP.Domain.Fees.Entities.FeeHead;

namespace SchoolERP.Application.Features.Fees.Builders;

/// <summary>
/// Converts an existing transport fee quote into an invoice line draft.
///
/// Responsibilities:
/// - Validate the transport quote.
/// - Resolve its recurrence frequency.
/// - Calculate occurrences within the billing period.
/// - Create an InvoiceLineDraft with frozen snapshots.
///
/// This is a pure in-memory builder:
/// - No database access.
/// - No transport assignment lookup.
/// - No invoice persistence.
/// - No concession calculation.
/// </summary>
public static class TransportFeeLineBuilder
{
    /// <summary>
    /// Creates a transport invoice line when the transport fee
    /// is applicable to the supplied billing period.
    ///
    /// Returns a successful null result when:
    /// - Transport amount is zero.
    /// - No transport occurrence falls within this period.
    ///
    /// Returns a validation error when the quote contains
    /// an unsupported frequency or invalid amount.
    /// </summary>
    public static Result<InvoiceLineDraft?> Build(
        TransportFeeQuote quote,
        FeeHeadEntity transportFeeHead,
        DateOnly academicYearStartDate,
        DateOnly academicYearEndDate,
        BillingPeriod billingPeriod)
    {
        // ---------------------------------------------------------
        // BLOCK 1: Validate required objects.
        //
        // A valid transport quote, FeeHead and billing period
        // are required to construct an invoice line.
        // ---------------------------------------------------------

        ArgumentNullException.ThrowIfNull(quote);
        ArgumentNullException.ThrowIfNull(transportFeeHead);
        ArgumentNullException.ThrowIfNull(billingPeriod);

        // ---------------------------------------------------------
        // BLOCK 2: Validate monetary amount.
        //
        // Negative transport charges are not supported.
        // A zero amount means there is nothing to invoice.
        // ---------------------------------------------------------

        if (quote.Amount < 0m)
        {
            return Error.Validation(
                "Transport fee amount cannot be negative.");
        }

        if (quote.Amount == 0m)
        {
            return Result.Success<InvoiceLineDraft?>(null);
        }

        // ---------------------------------------------------------
        // BLOCK 3: Validate the supplied Academic Year.
        //
        // Recurrence dates are anchored to Academic Year start.
        // ---------------------------------------------------------

        if (academicYearEndDate < academicYearStartDate)
        {
            return Error.Validation(
                "Academic Year end date cannot be earlier than start date.");
        }

        // ---------------------------------------------------------
        // BLOCK 4: Validate that the billing period belongs
        // to the supplied Academic Year.
        //
        // This prevents charging transport for dates outside
        // the selected Academic Year.
        // ---------------------------------------------------------

        if (billingPeriod.EndDate < billingPeriod.StartDate)
        {
            return Error.Validation(
                "Billing period end date cannot be earlier than start date.");
        }

        if (billingPeriod.StartDate < academicYearStartDate ||
            billingPeriod.EndDate > academicYearEndDate)
        {
            return Error.Validation(
                "Transport billing period must fall within the Academic Year.");
        }

        // ---------------------------------------------------------
        // BLOCK 5: Validate the transport FeeHead.
        //
        // The caller is responsible for loading the FeeHead.
        // This builder verifies that the supplied FeeHead
        // represents the code returned by the transport provider.
        // ---------------------------------------------------------

        if (transportFeeHead.DeletedAt != null ||
            transportFeeHead.Status != FeeHeadStatus.Active)
        {
            return Error.Validation(
                "Configured transport FeeHead is inactive or deleted.");
        }

        if (!string.Equals(
                transportFeeHead.Code,
                quote.TransportFeeHeadCode,
                StringComparison.OrdinalIgnoreCase))
        {
            return Error.Validation(
                "Transport FeeHead code does not match the transport quote.");
        }

        // ---------------------------------------------------------
        // BLOCK 6: Convert the provider's frequency string
        // into the existing Fees module FeeFrequency enum.
        //
        // The provider currently returns enum ToString() values
        // such as Monthly, Quarterly and HalfYearly.
        //
        // The documented database representations may also be:
        // monthly, quarterly and half_yearly.
        //
        // Both representations are accepted here.
        // ---------------------------------------------------------

        if (!TryMapFrequency(
                quote.Frequency,
                out var feeFrequency))
        {
            return Error.Validation(
                $"Unsupported transport fee frequency '{quote.Frequency}'.");
        }

        // ---------------------------------------------------------
        // BLOCK 7: Reuse the existing FeeHead frequency policy.
        //
        // This keeps recurrence calculations consistent between
        // ordinary FeeStructureLines and transport charges.
        //
        // Examples:
        // Monthly transport + Quarterly invoice = 3 occurrences.
        // Quarterly transport + Quarterly invoice = 1 occurrence.
        // HalfYearly transport + Monthly invoice = 1 occurrence
        // only at the beginning of each half-year.
        // ---------------------------------------------------------

        var occurrenceCount =
            FeeHeadFrequencyPolicy.GetOccurrenceCount(
                feeFrequency,
                academicYearStartDate,
                academicYearEndDate,
                billingPeriod);

        // No occurrence means transport should not be charged
        // in this particular invoice period.
        if (occurrenceCount == 0)
        {
            return Result.Success<InvoiceLineDraft?>(null);
        }

        // ---------------------------------------------------------
        // BLOCK 8: Calculate the transport line amount.
        //
        // Quote.Amount represents one recurrence charge.
        // Multiply it by the number of occurrences in the period.
        //
        // All monetary rounding uses the existing Money utility.
        // ---------------------------------------------------------

        var transportAmount = Money.Round(
            quote.Amount * occurrenceCount);

        if (transportAmount <= 0m)
        {
            return Result.Success<InvoiceLineDraft?>(null);
        }

        // ---------------------------------------------------------
        // BLOCK 9: Construct the transport invoice line snapshot.
        //
        // Transport is currently:
        // - Not eligible for concession calculation in this pipeline.
        // - Not subject to FeeStructure late-fee snapshots.
        //
        // The configured transport amount and FeeHead name
        // are frozen in the invoice draft.
        // ---------------------------------------------------------

        var lineDraft = new InvoiceLineDraft
        {
            FeeHeadId = transportFeeHead.Id,

            FeeHeadNameSnapshot = transportFeeHead.Name,

            Amount = transportAmount,

            ConcessionApplied = 0m,

            NetAmount = transportAmount,

            LateFeeApplicable = false,

            LateFeeTypeSnapshot = null,

            LateFeeValueSnapshot = null,

            LateFeeGraceDaysSnapshot = null,

            LateFeeCapPercentSnapshot = null,

            LateFeeSlabConfigSnapshot = null,

            LateFeeCapBase = transportAmount
        };

        // ---------------------------------------------------------
        // BLOCK 10: Return the completed line draft.
        //
        // The calling InvoiceBuilder will add this line
        // to its existing InvoiceDraft collection.
        // ---------------------------------------------------------

        return Result.Success<InvoiceLineDraft?>(lineDraft);
    }

    /// <summary>
    /// Maps the transport provider's frequency representation
    /// into the existing Fees module recurrence enum.
    ///
    /// Transport supports only:
    /// - Monthly
    /// - Quarterly
    /// - HalfYearly
    ///
    /// Annual and OneTime are not supported by the
    /// TransportFeeFrequency database constraint.
    /// </summary>
    private static bool TryMapFrequency(
        string? frequency,
        out FeeFrequency feeFrequency)
    {
        // Normalize casing and remove separators.
        // Examples:
        // "HalfYearly"  -> "halfyearly"
        // "half_yearly" -> "halfyearly"
        // "HALF-YEARLY" -> "halfyearly"
        var normalized = frequency?
            .Trim()
            .Replace("_", string.Empty)
            .Replace("-", string.Empty)
            .ToLowerInvariant();

        // Map only frequencies supported by the Transport module.
        feeFrequency = normalized switch
        {
            "monthly" => FeeFrequency.Monthly,

            "quarterly" => FeeFrequency.Quarterly,

            "halfyearly" => FeeFrequency.HalfYearly,

            _ => default
        };

        // Unknown or unsupported frequencies must not silently
        // generate an incorrect transport charge.
        return normalized is
            "monthly" or
            "quarterly" or
            "halfyearly";
    }
}
