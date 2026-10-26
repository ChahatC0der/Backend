using SchoolERP.Application.Common.Interfaces;

namespace SchoolERP.Infrastructure.Services;

/// <summary>
/// STUB — Phase 0. Full implementation lands in Phase 2 (atomic UPDLOCK on dbo.InvoiceSequences).
/// Registered now so DI resolves during Phase 1.
/// </summary>
public sealed class FeeSequenceGenerator : IFeeSequenceGenerator
{
    public Task<string> NextInvoiceNumberAsync(Guid branchId, string academicYearCode, CancellationToken ct = default)
        => throw new NotImplementedException("FeeSequenceGenerator: implemented in Phase 2.");

    public Task<string> NextReceiptNumberAsync(Guid branchId, string academicYearCode, CancellationToken ct = default)
        => throw new NotImplementedException("FeeSequenceGenerator: implemented in Phase 2.");
}
