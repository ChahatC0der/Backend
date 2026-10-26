namespace SchoolERP.Application.Common.Interfaces;

/// <summary>
/// Atomic invoice/receipt number generator backed by dbo.InvoiceSequences.
/// Invoice: {BranchCode}-{AYShort}-{seq:D6}     e.g. NCM-2526-000001
/// Receipt: {BranchCode}-RCP-{AYShort}-{seq:D6} e.g. NCM-RCP-2526-000001
/// Uniqueness scope: per (TenantId, BranchId), enforced by unique DB indexes.
/// </summary>
public interface IFeeSequenceGenerator
{
    Task<string> NextInvoiceNumberAsync(Guid branchId, string academicYearCode, CancellationToken ct = default);
    Task<string> NextReceiptNumberAsync(Guid branchId, string academicYearCode, CancellationToken ct = default);
}
