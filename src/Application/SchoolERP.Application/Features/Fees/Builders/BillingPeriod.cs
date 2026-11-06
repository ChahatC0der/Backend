using SchoolERP.Domain.Fees.Enums;

namespace SchoolERP.Application.Features.Fees.Builders;

/// <summary>
/// Represents one billing period within an academic year.
/// This is an in-memory model; it does not access or persist data.
/// </summary>
public sealed record BillingPeriod(
    int SequenceNumber,
    InvoiceGenerationFrequency Frequency,
    DateOnly StartDate,
    DateOnly EndDate
);