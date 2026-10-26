using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Fees.DTOs.Refund;
using SchoolERP.Domain.Fees.Enums;
using SchoolERP.Domain.Shared.Results;
using RefundEntity = SchoolERP.Domain.Fees.Entities.Refund;
using PaymentEntity = SchoolERP.Domain.Fees.Entities.Payment;

namespace SchoolERP.Application.Features.Fees.Commands.Refund.CompleteRefund;

public sealed class CompleteRefundCommandHandler : IRequestHandler<CompleteRefundCommand, Result<RefundResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;
    private readonly IWalletLedgerWriter _wallet;

    public CompleteRefundCommandHandler(
        IApplicationDbContext db, ICurrentTenantService tenant, IWalletLedgerWriter wallet)
    { _db = db; _tenant = tenant; _wallet = wallet; }

    public async Task<Result<RefundResponse>> Handle(CompleteRefundCommand request, CancellationToken ct)
    {
        var tenantId = _tenant.GetTenantId();
        if (tenantId == Guid.Empty) return Error.Unauthorized("Tenant context is not resolved.");

        var entity = await _db.Set<RefundEntity>().FirstOrDefaultAsync(r =>
            r.Id == request.RefundId && r.TenantId == tenantId, ct);
        if (entity is null) return Error.NotFound("Refund", request.RefundId.ToString());

        if (entity.Status != RefundStatus.Approved)
            return Error.Conflict($"Cannot complete a refund in status {entity.Status}. Must be approved first.");

        if (entity.PaymentId.HasValue)
        {
            // ---- Payment-linked refund: mark payment as refunded ----
            var payment = await _db.Set<PaymentEntity>()
                .FirstOrDefaultAsync(p => p.Id == entity.PaymentId.Value
                                     && p.TenantId == tenantId
                                     && p.DeletedAt == null, ct);
            if (payment is null) return Error.NotFound("Payment", entity.PaymentId.Value.ToString());

            if (payment.Status == PaymentStatus.Refunded)
                return Error.Conflict("Payment is already refunded.");

            payment.Status = PaymentStatus.Refunded;
            payment.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            // ---- Wallet refund: debit wallet via ledger ----
            var walletResult = await _wallet.ApplyAsync(new WalletOperation(
                StudentId: entity.StudentId,
                TenantId: tenantId,
                AcademicYearId: null,
                Delta: -entity.Amount,
                Type: CreditTransactionType.Refund,
                Reason: $"Refund #{entity.Id}: {entity.Reason}"), ct);
            if (walletResult.IsFailure) return walletResult.Error!;
        }

        entity.Status = RefundStatus.Completed;
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);

        // TODO (audit): await _auditWriter.LogAsync("Refunds", entity.Id, FinancialAuditAction.Update, null, new { Status = "completed" }, ct);

        return Result.Success(RefundResponse.FromEntity(entity));
    }
}
