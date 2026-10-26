using SchoolERP.Domain.Fees.Enums;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Common.Interfaces;

public sealed record WalletOperation(
    long StudentId,
    Guid TenantId,
    long? AcademicYearId,
    decimal Delta,
    CreditTransactionType Type,
    long? ReferenceInvoiceId = null,
    long? ReferencePaymentId = null,
    string? Reason = null);

public interface IWalletLedgerWriter
{
    Task<Result<decimal>> ApplyAsync(WalletOperation op, CancellationToken ct = default);

    /// <summary>Acquire UPDLOCK on the student row only (no ledger write).
    /// Use in Transfer flows to lock both students in ID-ascending order and prevent deadlocks.</summary>
    Task<Result> LockStudentAsync(long studentId, CancellationToken ct = default);
}
