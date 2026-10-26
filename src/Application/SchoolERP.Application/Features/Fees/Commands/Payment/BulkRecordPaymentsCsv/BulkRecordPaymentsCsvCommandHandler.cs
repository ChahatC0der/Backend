using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Fees.Builders;
using SchoolERP.Application.Features.Fees.DTOs.Payment;
using SchoolERP.Domain.Fees.ValueObjects;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Fees.Commands.Payment.BulkRecordPaymentsCsv;

/// <summary>
/// All-or-nothing per request (Phase 1). Validates ALL rows first  if any row fails,
/// returns aggregate error and writes nothing. Cap 500 rows.
/// </summary>
public sealed class BulkRecordPaymentsCsvCommandHandler : IRequestHandler<BulkRecordPaymentsCsvCommand, Result<BulkCsvResult>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;
    private readonly ICurrentBranchService _branch;
    private readonly ICurrentUserService _user;
    private readonly ICurrentAcademicYearProvider _ay;
    private readonly IFeeSequenceGenerator _sequence;
    private readonly IWalletLedgerWriter _wallet;

    public BulkRecordPaymentsCsvCommandHandler(
        IApplicationDbContext db, ICurrentTenantService tenant, ICurrentBranchService branch,
        ICurrentUserService user, ICurrentAcademicYearProvider ay,
        IFeeSequenceGenerator sequence, IWalletLedgerWriter wallet)
    {
        _db = db; _tenant = tenant; _branch = branch; _user = user;
        _ay = ay; _sequence = sequence; _wallet = wallet;
    }

    public async Task<Result<BulkCsvResult>> Handle(BulkRecordPaymentsCsvCommand request, CancellationToken ct)
    {
        var scope = FeeScopeGuard.RequireBranch(_tenant, _branch);
        if (scope.IsFailure) return scope.Error!;
        var (tenantId, branchId) = scope.Value;

        var ay = await _ay.GetByIdAsync(request.AcademicYearId, ct);
        if (ay is null) return Error.NotFound("AcademicYear", request.AcademicYearId.ToString());

        if (request.Rows.Count > 500)
            return Error.Validation("Bulk payment upload is capped at 500 rows per request in Phase 1.");

        // ---- Phase 1: validate ALL rows into memory (no writes) ----
        var built = new List<PaymentCollector.BuildResult>();
        var rowErrors = new List<string>();

        for (int i = 0; i < request.Rows.Count; i++)
        {
            var row = request.Rows[i];
            var req = new CollectPaymentRequest
            {
                StudentId = row.StudentId,
                AcademicYearId = request.AcademicYearId,
                PaymentDate = row.PaymentDate,
                TotalAmountReceived = row.Amount,
                Mode = row.Mode,
                ReferenceNumber = row.ReferenceNumber,
                Notes = row.Notes,
                Allocations = row.Allocations,
            };

            var b = await PaymentCollector.BuildAsync(_db, _sequence, _user.GetUserId(),
                tenantId, branchId, ay.Code, req, ct);

            if (b.IsFailure)
            {
                rowErrors.Add($"Row {i + 1}: {b.Error!.ToString()}");
                continue;
            }
            built.Add(b.Value);
        }

        if (rowErrors.Count > 0)
        {
            var preview = string.Join(" | ", rowErrors.Take(5));
            var more = rowErrors.Count > 5 ? $" (+{rowErrors.Count - 5} more)" : "";
            return Error.Validation($"Bulk upload rejected  all-or-nothing mode. Failures: {preview}{more}");
        }

        // ---- Phase 2: persist all payments (populate Ids) ----
        await _db.SaveChangesAsync(ct);

        // ---- Phase 3: finalize all (invoice updates + wallet credits) ----
        foreach (var b in built)
        {
            var f = await PaymentCollector.FinalizeAsync(_db, _wallet, b, ct);
            if (f.IsFailure) return f.Error!;
        }

        await _db.SaveChangesAsync(ct);

        // TODO (audit): await _auditWriter.LogAsync("Payments", 0, FinancialAuditAction.Create, null, new { Count = built.Count, AcademicYearId = request.AcademicYearId }, ct);

        return Result.Success(new BulkCsvResult(
            Total: request.Rows.Count,
            Inserted: built.Count,
            PaymentIds: built.Select(b => b.Payment.Id).ToList()));
    }
}
