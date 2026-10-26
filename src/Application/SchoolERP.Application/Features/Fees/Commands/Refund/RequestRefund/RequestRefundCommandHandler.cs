using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Fees.Builders;
using SchoolERP.Application.Features.Fees.DTOs.Refund;
using SchoolERP.Domain.Fees.Enums;
using SchoolERP.Domain.Fees.ValueObjects;
using SchoolERP.Domain.Shared.Results;
using RefundEntity = SchoolERP.Domain.Fees.Entities.Refund;
using PaymentEntity = SchoolERP.Domain.Fees.Entities.Payment;
using StudentCreditLedgerEntity = SchoolERP.Domain.Fees.Entities.StudentCreditLedger;

namespace SchoolERP.Application.Features.Fees.Commands.Refund.RequestRefund;

public sealed class RequestRefundCommandHandler : IRequestHandler<RequestRefundCommand, Result<RefundResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;

    public RequestRefundCommandHandler(IApplicationDbContext db, ICurrentTenantService tenant)
    { _db = db; _tenant = tenant; }

    public async Task<Result<RefundResponse>> Handle(RequestRefundCommand request, CancellationToken ct)
    {
        var tenantId = _tenant.GetTenantId();
        if (tenantId == Guid.Empty) return Error.Unauthorized("Tenant context is not resolved.");

        if (!TryParseReason(request.Request.Reason, out var reason))
            return Error.Validation($"Invalid refund reason: {request.Request.Reason}.");
        if (!TryParseMode(request.Request.PaymentMode, out var mode))
            return Error.Validation($"Invalid payment mode: {request.Request.PaymentMode}.");

        // ---- Payment-linked refund: full amount only ----
        if (request.Request.PaymentId.HasValue)
        {
            var payment = await _db.Set<PaymentEntity>().AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == request.Request.PaymentId.Value
                                     && p.TenantId == tenantId
                                     && p.DeletedAt == null, ct);
            if (payment is null) return Error.NotFound("Payment", request.Request.PaymentId.Value.ToString());

            if (payment.StudentId != request.Request.StudentId)
                return Error.Validation("Payment does not belong to the specified student.");

            if (payment.Status is PaymentStatus.Bounced)
                return Error.Conflict("Cannot refund a bounced payment.");

            if (payment.Status is PaymentStatus.Refunded)
                return Error.Conflict("Payment is already refunded.");

            if (request.Request.Amount != payment.Amount)
                return Error.Validation($"Phase 1 supports full-payment refunds only. Payment amount is {payment.Amount}.");
        }
        else
        {
            // ---- Wallet refund: partial OK, up to current balance ----
            var walletBalance = await _db.Set<StudentCreditLedgerEntity>().AsNoTracking()
                .Where(l => l.StudentId == request.Request.StudentId && l.TenantId == tenantId)
                .OrderByDescending(l => l.Id)
                .Select(l => (decimal?)l.BalanceAfter)
                .FirstOrDefaultAsync(ct) ?? 0m;

            if (walletBalance <= 0)
                return Error.Validation("Student has no wallet balance to refund.");
            if (request.Request.Amount > walletBalance)
                return Error.Validation($"Refund amount {request.Request.Amount} exceeds wallet balance {walletBalance}.");
        }

        var entity = new RefundEntity
        {
            TenantId = tenantId,
            StudentId = request.Request.StudentId,
            PaymentId = request.Request.PaymentId,
            Amount = Money.Round(request.Request.Amount),
            Reason = reason,
            PaymentMode = mode,
            ReferenceNumber = request.Request.ReferenceNumber,
            Status = RefundStatus.Pending,
            CreatedAt = DateTime.UtcNow,
        };
        _db.Set<RefundEntity>().Add(entity);
        await _db.SaveChangesAsync(ct);

        // TODO (audit): await _auditWriter.LogAsync("Refunds", entity.Id, FinancialAuditAction.Create, null, entity, ct);

        return Result.Success(RefundResponse.FromEntity(entity));
    }

    internal static bool TryParseReason(string value, out RefundReason r)
    {
        foreach (var kv in EnumMaps_JsonAdapter.RefundReasonMap)
            if (string.Equals(kv.Value, value, StringComparison.OrdinalIgnoreCase)) { r = kv.Key; return true; }
        r = default; return false;
    }

    internal static bool TryParseMode(string value, out PaymentMode m)
    {
        foreach (var kv in EnumMaps_JsonAdapter.PaymentModeMap)
            if (string.Equals(kv.Value, value, StringComparison.OrdinalIgnoreCase)) { m = kv.Key; return true; }
        m = default; return false;
    }
}
