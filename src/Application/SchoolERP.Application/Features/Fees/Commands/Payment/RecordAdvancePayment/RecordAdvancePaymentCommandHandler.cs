using MediatR;
using SchoolERP.Application.Features.Fees.Builders;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Fees.DTOs.Payment;
using SchoolERP.Domain.Fees.Enums;
using SchoolERP.Domain.Shared.Results;
using PaymentEntity = SchoolERP.Domain.Fees.Entities.Payment;

namespace SchoolERP.Application.Features.Fees.Commands.Payment.RecordAdvancePayment;

public sealed class RecordAdvancePaymentCommandHandler : IRequestHandler<RecordAdvancePaymentCommand, Result<PaymentResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;
    private readonly ICurrentBranchService _branch;
    private readonly ICurrentUserService _user;
    private readonly ICurrentAcademicYearProvider _ay;
    private readonly IFeeSequenceGenerator _sequence;
    private readonly IWalletLedgerWriter _wallet;

    public RecordAdvancePaymentCommandHandler(
        IApplicationDbContext db, ICurrentTenantService tenant, ICurrentBranchService branch,
        ICurrentUserService user, ICurrentAcademicYearProvider ay,
        IFeeSequenceGenerator sequence, IWalletLedgerWriter wallet)
    {
        _db = db; _tenant = tenant; _branch = branch; _user = user;
        _ay = ay; _sequence = sequence; _wallet = wallet;
    }

    public async Task<Result<PaymentResponse>> Handle(RecordAdvancePaymentCommand request, CancellationToken ct)
    {
        var scope = FeeScopeGuard.RequireBranch(_tenant, _branch);
        if (scope.IsFailure) return scope.Error!;
        var (tenantId, branchId) = scope.Value;

        if (!TryParseMode(request.Request.Mode, out var mode))
            return Error.Validation($"Invalid payment mode: {request.Request.Mode}.");

        long ayId; string ayCode;
        if (request.Request.AcademicYearId.HasValue)
        {
            var a = await _ay.GetByIdAsync(request.Request.AcademicYearId.Value, ct);
            if (a is null) return Error.NotFound("AcademicYear", request.Request.AcademicYearId.Value.ToString());
            ayId = a.Id; ayCode = a.Code;
        }
        else
        {
            var a = await _ay.GetCurrentAsync(ct);
            if (a is null) return Error.Validation("No current academic year is configured for this branch.");
            ayId = a.Id; ayCode = a.Code;
        }

        var receiptNumber = await _sequence.NextReceiptNumberAsync(branchId, ayCode, ct);

        var payment = new PaymentEntity
        {
            TenantId = tenantId,
            BranchId = branchId,
            StudentId = request.Request.StudentId,
            ReceiptNumber = receiptNumber,
            PaymentDate = request.Request.PaymentDate == default ? DateTime.UtcNow : request.Request.PaymentDate,
            Amount = request.Request.Amount,
            AllocatedAmount = 0m,
            UnallocatedAmount = request.Request.Amount,
            PaymentSource = PaymentSource.Counter,
            Mode = mode,
            ReferenceNumber = request.Request.ReferenceNumber,
            Notes = request.Request.Notes,
            CollectedBy = _user.GetUserId(),
            Status = mode == PaymentMode.Cheque ? PaymentStatus.Pending : PaymentStatus.Completed,
            CreatedAt = DateTime.UtcNow,
        };
        _db.Set<PaymentEntity>().Add(payment);
        await _db.SaveChangesAsync(ct);

        var walletResult = await _wallet.ApplyAsync(new WalletOperation(
            StudentId: request.Request.StudentId,
            TenantId: tenantId,
            AcademicYearId: ayId,
            Delta: request.Request.Amount,
            Type: CreditTransactionType.AdvancePayment,
            ReferencePaymentId: payment.Id,
            Reason: string.IsNullOrWhiteSpace(request.Request.Notes) ? "Advance payment" : request.Request.Notes), ct);
        if (walletResult.IsFailure) return walletResult.Error!;

        await _db.SaveChangesAsync(ct);

        // TODO (audit): await _auditWriter.LogAsync("Payments", payment.Id, FinancialAuditAction.Create, null, payment, ct);

        return Result.Success(PaymentResponse.FromEntity(payment));
    }

    internal static bool TryParseMode(string value, out PaymentMode mode)
    {
        foreach (var kv in EnumMaps_JsonAdapter.PaymentModeMap)
            if (string.Equals(kv.Value, value, StringComparison.OrdinalIgnoreCase)) { mode = kv.Key; return true; }
        mode = default; return false;
    }
}
