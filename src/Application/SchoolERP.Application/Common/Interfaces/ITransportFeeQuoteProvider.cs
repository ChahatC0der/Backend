namespace SchoolERP.Application.Common.Interfaces;

/// <summary>Effective transport fee for a student — resolved by Infrastructure from active assignment + transport fee config.</summary>
public sealed record TransportFeeQuote(
    decimal Amount,
    string Frequency,               // monthly | quarterly | half_yearly
    long? RouteId,
    long? StopId,
    string TransportFeeHeadCode);   // always "TRANSPORT" in Phase 1

/// <summary>
/// Cross-module contract: Fees InvoiceBuilder calls this to append a transport fee line.
/// Implementation MUST return null when:
///   - No active assignment for (student, AY), OR
///   - No transport fee configured for that assignment.
/// </summary>
public interface ITransportFeeQuoteProvider
{
    Task<TransportFeeQuote?> GetQuoteAsync(long studentId, long academicYearId, CancellationToken ct = default);
}
