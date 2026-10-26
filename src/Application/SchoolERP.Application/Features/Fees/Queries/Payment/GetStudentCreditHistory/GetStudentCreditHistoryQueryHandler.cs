using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Fees.DTOs.Payment;
using SchoolERP.Domain.Shared.Results;
using StudentCreditLedgerEntity = SchoolERP.Domain.Fees.Entities.StudentCreditLedger;

namespace SchoolERP.Application.Features.Fees.Queries.Payment.GetStudentCreditHistory;

public sealed class GetStudentCreditHistoryQueryHandler : IRequestHandler<GetStudentCreditHistoryQuery, Result<CreditHistoryResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;

    public GetStudentCreditHistoryQueryHandler(IApplicationDbContext db, ICurrentTenantService tenant)
    { _db = db; _tenant = tenant; }

    public async Task<Result<CreditHistoryResponse>> Handle(GetStudentCreditHistoryQuery request, CancellationToken ct)
    {
        var tenantId = _tenant.GetTenantId();
        if (tenantId == Guid.Empty) return Error.Unauthorized("Tenant context is not resolved.");

        var rows = await _db.Set<StudentCreditLedgerEntity>().AsNoTracking()
            .Where(l => l.StudentId == request.StudentId && l.TenantId == tenantId)
            .OrderByDescending(l => l.Id)
            .Take(500)
            .ToListAsync(ct);

        var currentBalance = rows.Count > 0 ? rows[0].BalanceAfter : 0m;

        return Result.Success(new CreditHistoryResponse
        {
            StudentId = request.StudentId,
            CurrentBalance = currentBalance,
            Ledger = rows.Select(l => new CreditLedgerRow
            {
                Id = l.Id,
                CreatedAt = l.CreatedAt,
                Amount = l.Amount,
                BalanceAfter = l.BalanceAfter,
                TransactionType = l.TransactionType.ToString(),
                ReferenceInvoiceId = l.ReferenceInvoiceId,
                ReferencePaymentId = l.ReferencePaymentId,
                Reason = l.Reason,
            }).ToList(),
        });
    }
}
