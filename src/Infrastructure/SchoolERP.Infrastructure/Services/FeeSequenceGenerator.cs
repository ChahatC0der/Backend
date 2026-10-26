using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Fees.Builders;
using SchoolERP.Infrastructure.Persistence;
using InvoiceSequenceEntity = SchoolERP.Domain.Fees.Entities.InvoiceSequence;

namespace SchoolERP.Infrastructure.Services;

public sealed class FeeSequenceGenerator : IFeeSequenceGenerator
{
    private const string InvoiceType = "INVOICE";
    private const string ReceiptType = "RECEIPT";

    private readonly AppDbContext _db;
    public FeeSequenceGenerator(AppDbContext db) => _db = db;

    public async Task<string> NextInvoiceNumberAsync(Guid branchId, string academicYearCode, CancellationToken ct = default)
    {
        var seq = await IncrementAsync(branchId, academicYearCode, InvoiceType, ct);
        var code = await GetBranchCodeAsync(branchId, ct);
        return InvoiceNumberFormatter.FormatInvoice(code, academicYearCode, seq);
    }

    public async Task<string> NextReceiptNumberAsync(Guid branchId, string academicYearCode, CancellationToken ct = default)
    {
        var seq = await IncrementAsync(branchId, academicYearCode, ReceiptType, ct);
        var code = await GetBranchCodeAsync(branchId, ct);
        return InvoiceNumberFormatter.FormatReceipt(code, academicYearCode, seq);
    }

    /// <summary>
    /// Atomically increments the sequence row for (branchId, ayCode, type).
    /// The UPDLOCK + HOLDLOCK hint takes an exclusive row lock that is held until the
    /// ambient transaction (opened by TransactionBehavior) commits, serialising concurrent callers.
    /// If no row exists, HOLDLOCK range-locks the gap so concurrent first-inserts cannot race.
    /// The update/insert is persisted via SaveChangesAsync — either by TransactionBehavior at the
    /// end of the command pipeline (ambient case) or immediately in the no-ambient case below.
    /// </summary>
    private async Task<long> IncrementAsync(Guid branchId, string ayCode, string type, CancellationToken ct)
    {
        if (_db.Database.CurrentTransaction is not null)
        {
            // Ambient transaction exists — TransactionBehavior will SaveChangesAsync at the end.
            return await IncrementCoreAsync(branchId, ayCode, type, ct);
        }

        // No ambient transaction — own one so the lock/update pair is atomic.
        await using var tx = await _db.Database.BeginTransactionAsync(ct);
        var result = await IncrementCoreAsync(branchId, ayCode, type, ct);
        await _db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return result;
    }

    private async Task<long> IncrementCoreAsync(Guid branchId, string ayCode, string type, CancellationToken ct)
    {
        var existing = await _db.Set<InvoiceSequenceEntity>()
            .FromSqlInterpolated($@"
                SELECT * FROM dbo.InvoiceSequences WITH (UPDLOCK, HOLDLOCK)
                WHERE BranchId = {branchId}
                  AND AcademicYearCode = {ayCode}
                  AND SequenceType = {type}")
            .FirstOrDefaultAsync(ct);

        if (existing is null)
        {
            _db.Set<InvoiceSequenceEntity>().Add(new InvoiceSequenceEntity
            {
                BranchId = branchId,
                AcademicYearCode = ayCode,
                SequenceType = type,
                CurrentSequence = 1,
            });
            return 1;
        }

        existing.CurrentSequence += 1;
        return existing.CurrentSequence;
    }

    private async Task<string> GetBranchCodeAsync(Guid branchId, CancellationToken ct)
    {
        var row = await _db.Set<SchoolERP.Domain.Tenants.Entities.Branch>().AsNoTracking()
            .Where(b => b.Id == branchId)
            .Select(b => new { b.Code, b.Name })
            .FirstOrDefaultAsync(ct);

        if (row is null) throw new InvalidOperationException($"Branch {branchId} not found.");

        if (!string.IsNullOrWhiteSpace(row.Code)) return row.Code.ToUpperInvariant();
        if (!string.IsNullOrWhiteSpace(row.Name))
        {
            var t = row.Name.Trim().ToUpperInvariant();
            return t.Length <= 3 ? t : t[..3];
        }
        return "BRN";
    }
}
