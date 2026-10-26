using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.DTOs;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Fees.DTOs.Payment;
using SchoolERP.Domain.Shared.Results;
using PaymentEntity = SchoolERP.Domain.Fees.Entities.Payment;

namespace SchoolERP.Application.Features.Fees.Queries.Payment.GetPayments;

public sealed class GetPaymentsQueryHandler : IRequestHandler<GetPaymentsQuery, Result<PagedResponse<PaymentResponse>>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;
    private readonly ICurrentBranchService _branch;

    public GetPaymentsQueryHandler(IApplicationDbContext db, ICurrentTenantService tenant, ICurrentBranchService branch)
    { _db = db; _tenant = tenant; _branch = branch; }

    public async Task<Result<PagedResponse<PaymentResponse>>> Handle(GetPaymentsQuery request, CancellationToken ct)
    {
        var scope = FeeScopeGuard.RequireBranch(_tenant, _branch);
        if (scope.IsFailure) return scope.Error!;
        var (tenantId, branchId) = scope.Value;

        var page = request.Page <= 0 ? 1 : request.Page;
        var size = request.Size <= 0 ? 10 : Math.Min(request.Size, 200);

        var q = _db.Set<PaymentEntity>().AsNoTracking()
            .Where(p => p.TenantId == tenantId && p.BranchId == branchId && p.DeletedAt == null);

        if (request.StudentId.HasValue) q = q.Where(p => p.StudentId == request.StudentId.Value);
        if (request.Status.HasValue) q = q.Where(p => p.Status == request.Status.Value);
        if (request.From.HasValue) q = q.Where(p => p.PaymentDate >= request.From.Value);
        if (request.To.HasValue) q = q.Where(p => p.PaymentDate <= request.To.Value);

        var total = await q.CountAsync(ct);
        var rows = await q.OrderByDescending(p => p.PaymentDate).ThenByDescending(p => p.Id)
            .Skip((page - 1) * size).Take(size).ToListAsync(ct);

        return Result.Success(new PagedResponse<PaymentResponse>
        {
            Data = rows.Select(PaymentResponse.FromEntity).ToList(),
            Page = page, Size = size, TotalCount = total,
            //TotalPages = (int)Math.Ceiling(total / (double)size),
        });
    }
}
