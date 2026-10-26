using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Domain.Fees.ValueObjects;
using SchoolERP.Domain.Shared.Results;
using SchoolERP.Infrastructure.Persistence;
using StudentCreditLedgerEntity = SchoolERP.Domain.Fees.Entities.StudentCreditLedger;

namespace SchoolERP.Infrastructure.Services;

public sealed class WalletLedgerWriter : IWalletLedgerWriter
{
    private readonly AppDbContext _db;
    public WalletLedgerWriter(AppDbContext db) => _db = db;

    public async Task<Result> LockStudentAsync(long studentId, CancellationToken ct = default)
    {
        var dbTx = _db.Database.CurrentTransaction?.GetDbTransaction();
        var conn = _db.Database.GetDbConnection();

        var lockedId = await conn.ExecuteScalarAsync<long?>(new CommandDefinition(
            "SELECT Id FROM dbo.Students WITH (UPDLOCK, ROWLOCK) WHERE Id = @sid",
            new { sid = studentId }, transaction: dbTx, cancellationToken: ct));

        if (lockedId is null) return Result.Failure(Error.NotFound("Student", studentId.ToString()));
        return Result.Success(true);
    }

    public async Task<Result<decimal>> ApplyAsync(WalletOperation op, CancellationToken ct = default)
    {
        var lockResult = await LockStudentAsync(op.StudentId, ct);
        if (lockResult.IsFailure) return lockResult.Error!;

        var current = await _db.Set<StudentCreditLedgerEntity>().AsNoTracking()
            .Where(l => l.StudentId == op.StudentId && l.TenantId == op.TenantId)
            .OrderByDescending(l => l.Id)
            .Select(l => (decimal?)l.BalanceAfter)
            .FirstOrDefaultAsync(ct) ?? 0m;

        var newBalance = Money.Round(current + op.Delta);
        if (newBalance < 0)
            return Error.Validation($"Insufficient wallet balance. Available: {current}, requested deduction: {(-op.Delta)}.");

        _db.Set<StudentCreditLedgerEntity>().Add(new StudentCreditLedgerEntity
        {
            TenantId = op.TenantId,
            StudentId = op.StudentId,
            AcademicYearId = op.AcademicYearId,
            Amount = Money.Round(op.Delta),
            BalanceAfter = newBalance,
            TransactionType = op.Type,
            ReferenceInvoiceId = op.ReferenceInvoiceId,
            ReferencePaymentId = op.ReferencePaymentId,
            Reason = op.Reason,
            CreatedAt = DateTime.UtcNow,
        });

        return Result.Success(newBalance);
    }
}
