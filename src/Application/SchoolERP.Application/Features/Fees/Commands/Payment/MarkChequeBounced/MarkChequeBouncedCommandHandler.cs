using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Fees.DTOs.Payment;
using SchoolERP.Domain.Fees.Enums;
using SchoolERP.Domain.Fees.ValueObjects;
using SchoolERP.Domain.Shared.Results;
using PaymentEntity = SchoolERP.Domain.Fees.Entities.Payment;
using InvoiceEntity = SchoolERP.Domain.Fees.Entities.Invoice;

namespace SchoolERP.Application.Features.Fees.Commands.Payment.MarkChequeBounced;

public sealed class MarkChequeBouncedCommandHandler : IRequestHandler<MarkChequeBouncedCommand, Result<PaymentResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;
    private readonly ICurrentBranchService _branch;
    private readonly IWalletLedgerWriter _wallet;

    public MarkChequeBouncedCommandHandler(
        IApplicationDbContext db, ICurrentTenantService tenant,
        ICurrentBranchService branch, IWalletLedgerWriter wallet)
    { _db = db; _tenant = tenant; _branch = branch; _wallet = wallet; }

    public async Task<Result<PaymentResponse>> Handle(MarkChequeBouncedCommand request, CancellationToken ct)
    {
        var scope = FeeScopeGuard.RequireBranch(_tenant, _branch);
        if (scope.IsFailure) return scope.Error!;
        var (tenantId, branchId) = scope.Value;

        var payment = await _db.Set<PaymentEntity>().Include(p => p.Allocations)
            .FirstOrDefaultAsync(p => p.Id == request.PaymentId
                                 && p.TenantId == tenantId
                                 && p.BranchId == branchId
                                 && p.DeletedAt == null, ct);
        if (payment is null) return Error.NotFound("Payment", request.PaymentId.ToString());

        if (payment.Status is PaymentStatus.Bounced or PaymentStatus.Refunded)
            return Error.Conflict($"Payment is already {payment.Status}.");

        // Reverse each allocation against its invoice.
        if (payment.Allocations.Count > 0)
        {
            var invoiceIds = payment.Allocations.Select(a => a.InvoiceId).ToList();
            var invoices = await _db.Set<InvoiceEntity>()
                .Where(i => invoiceIds.Contains(i.Id)
                         && i.TenantId == tenantId
                         && i.BranchId == branchId)
                .ToListAsync(ct);

            foreach (var alloc in payment.Allocations)
            {
                var invoice = invoices.FirstOrDefault(i => i.Id == alloc.InvoiceId);
                if (invoice is null) continue;

                invoice.PaidAmount = Money.Round(Math.Max(0m, invoice.PaidAmount - alloc.AllocatedAmount));
                invoice.Status = invoice.PaidAmount + invoice.CreditAppliedAmount >= invoice.NetPayable
                    ? InvoiceStatus.Paid
                    : (invoice.PaidAmount + invoice.CreditAppliedAmount == 0m
                        ? (invoice.DueDate < DateTime.UtcNow ? InvoiceStatus.Overdue : InvoiceStatus.Sent)
                        : InvoiceStatus.PartiallyPaid);
                invoice.UpdatedAt = DateTime.UtcNow;
            }
        }

        // Reverse any excess that went to wallet.
        if (payment.UnallocatedAmount > 0)
        {
            var walletResult = await _wallet.ApplyAsync(new WalletOperation(
                StudentId: payment.StudentId,
                TenantId: tenantId,
                AcademicYearId: null,
                Delta: -payment.UnallocatedAmount,
                Type: CreditTransactionType.ManualAdjustment,
                ReferencePaymentId: payment.Id,
                Reason: $"Cheque bounce reversal: {request.Reason}"), ct);
            if (walletResult.IsFailure) return walletResult.Error!;
        }

        payment.Status = PaymentStatus.Bounced;
        payment.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);

        // TODO (audit): await _auditWriter.LogAsync("Payments", payment.Id, FinancialAuditAction.Reverse, null, new { Reason = request.Reason }, ct);

        return Result.Success(PaymentResponse.FromEntity(payment));
    }
}
