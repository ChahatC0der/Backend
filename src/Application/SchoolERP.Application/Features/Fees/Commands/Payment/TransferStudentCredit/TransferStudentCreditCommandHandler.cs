using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Fees.DTOs.Payment;
using SchoolERP.Domain.Fees.Enums;
using SchoolERP.Domain.Shared.Results;
using StudentEntity = SchoolERP.Domain.Student.Entities.Student;

namespace SchoolERP.Application.Features.Fees.Commands.Payment.TransferStudentCredit;

public sealed class TransferStudentCreditCommandHandler : IRequestHandler<TransferStudentCreditCommand, Result<TransferCreditResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;
    private readonly IWalletLedgerWriter _wallet;

    public TransferStudentCreditCommandHandler(
        IApplicationDbContext db, ICurrentTenantService tenant, IWalletLedgerWriter wallet)
    { _db = db; _tenant = tenant; _wallet = wallet; }

    public async Task<Result<TransferCreditResponse>> Handle(TransferStudentCreditCommand request, CancellationToken ct)
    {
        var tenantId = _tenant.GetTenantId();
        if (tenantId == Guid.Empty) return Error.Unauthorized("Tenant context is not resolved.");

        var fromId = request.Request.FromStudentId;
        var toId = request.Request.ToStudentId;
        var amount = request.Request.Amount;

        // Both students must exist and be in the same family (siblings).
        var fromStudent = await _db.Set<StudentEntity>().AsNoTracking()
            .Where(s => s.Id == fromId).Select(s => new { s.Id, s.FamilyId }).FirstOrDefaultAsync(ct);
        if (fromStudent is null) return Error.NotFound("Student", fromId.ToString());

        var toStudent = await _db.Set<StudentEntity>().AsNoTracking()
            .Where(s => s.Id == toId).Select(s => new { s.Id, s.FamilyId }).FirstOrDefaultAsync(ct);
        if (toStudent is null) return Error.NotFound("Student", toId.ToString());

        if (fromStudent.FamilyId is null || toStudent.FamilyId != fromStudent.FamilyId)
            return Error.Validation("Transfers are only allowed between siblings in the same family.");

        // Lock in ID-ascending order to prevent deadlocks.
        var (first, second) = fromId < toId ? (fromId, toId) : (toId, fromId);
        var lock1 = await _wallet.LockStudentAsync(first, ct);
        if (lock1.IsFailure) return lock1.Error!;
        var lock2 = await _wallet.LockStudentAsync(second, ct);
        if (lock2.IsFailure) return lock2.Error!;

        var outResult = await _wallet.ApplyAsync(new WalletOperation(
            StudentId: fromId, TenantId: tenantId, AcademicYearId: null,
            Delta: -amount, Type: CreditTransactionType.ManualAdjustment,
            Reason: $"Transfer to student {toId}: {request.Request.Reason}"), ct);
        if (outResult.IsFailure) return outResult.Error!;

        var inResult = await _wallet.ApplyAsync(new WalletOperation(
            StudentId: toId, TenantId: tenantId, AcademicYearId: null,
            Delta: amount, Type: CreditTransactionType.ManualAdjustment,
            Reason: $"Transfer from student {fromId}: {request.Request.Reason}"), ct);
        if (inResult.IsFailure) return inResult.Error!;

        await _db.SaveChangesAsync(ct);

        // TODO (audit): await _auditWriter.LogAsync("StudentCreditLedger", 0, FinancialAuditAction.Adjust, null, new { From = fromId, To = toId, Amount = amount }, ct);

        return Result.Success(new TransferCreditResponse(fromId, toId, amount, outResult.Value, inResult.Value));
    }
}
