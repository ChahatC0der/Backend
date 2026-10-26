using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Fees.DTOs.Payment;
using SchoolERP.Domain.Shared.Results;
using PaymentEntity = SchoolERP.Domain.Fees.Entities.Payment;
using InvoiceEntity = SchoolERP.Domain.Fees.Entities.Invoice;
using StudentCreditLedgerEntity = SchoolERP.Domain.Fees.Entities.StudentCreditLedger;

namespace SchoolERP.Application.Features.Fees.Queries.Payment.GetPaymentReceipt;

public sealed class GetPaymentReceiptQueryHandler : IRequestHandler<GetPaymentReceiptQuery, Result<ReceiptResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;
    private readonly ICurrentBranchService _branch;

    public GetPaymentReceiptQueryHandler(IApplicationDbContext db, ICurrentTenantService tenant, ICurrentBranchService branch)
    { _db = db; _tenant = tenant; _branch = branch; }

    public async Task<Result<ReceiptResponse>> Handle(GetPaymentReceiptQuery request, CancellationToken ct)
    {
        var scope = FeeScopeGuard.RequireBranch(_tenant, _branch);
        if (scope.IsFailure) return scope.Error!;
        var (tenantId, branchId) = scope.Value;

        var payment = await _db.Set<PaymentEntity>().AsNoTracking()
            .Include(p => p.Allocations)
            .FirstOrDefaultAsync(p => p.Id == request.PaymentId
                                 && p.TenantId == tenantId
                                 && p.BranchId == branchId
                                 && p.DeletedAt == null, ct);
        if (payment is null) return Error.NotFound("Payment", request.PaymentId.ToString());

        var invoiceIds = payment.Allocations.Select(a => a.InvoiceId).ToList();
        var invoices = await _db.Set<InvoiceEntity>().AsNoTracking()
            .Where(i => invoiceIds.Contains(i.Id))
            .ToDictionaryAsync(i => i.Id, i => i.InvoiceNumber, ct);

        var walletBalance = await _db.Set<StudentCreditLedgerEntity>().AsNoTracking()
            .Where(l => l.StudentId == payment.StudentId && l.TenantId == tenantId)
            .OrderByDescending(l => l.Id)
            .Select(l => (decimal?)l.BalanceAfter)
            .FirstOrDefaultAsync(ct) ?? 0m;

        return Result.Success(new ReceiptResponse
        {
            Payment = PaymentResponse.FromEntity(payment),
            Allocations = payment.Allocations.Select(a => new ReceiptAllocationLine
            {
                InvoiceId = a.InvoiceId,
                InvoiceNumber = invoices.TryGetValue(a.InvoiceId, out var n) ? n : string.Empty,
                AllocatedAmount = a.AllocatedAmount,
            }).ToList(),
            WalletBalanceAfter = walletBalance,
        });
    }
}
