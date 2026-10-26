using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.DTOs;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Fees.DTOs.Refund;
using SchoolERP.Domain.Shared.Results;
using RefundEntity = SchoolERP.Domain.Fees.Entities.Refund;

namespace SchoolERP.Application.Features.Fees.Queries.Refund.GetRefunds;

public sealed class GetRefundsQueryHandler : IRequestHandler<GetRefundsQuery, Result<PagedResponse<RefundResponse>>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;

    public GetRefundsQueryHandler(IApplicationDbContext db, ICurrentTenantService tenant)
    { _db = db; _tenant = tenant; }

    public async Task<Result<PagedResponse<RefundResponse>>> Handle(GetRefundsQuery request, CancellationToken ct)
    {
        var tenantId = _tenant.GetTenantId();
        if (tenantId == Guid.Empty) return Error.Unauthorized("Tenant context is not resolved.");

        var page = request.Page <= 0 ? 1 : request.Page;
        var size = request.Size <= 0 ? 10 : Math.Min(request.Size, 200);

        var q = _db.Set<RefundEntity>().AsNoTracking().Where(r => r.TenantId == tenantId);
        if (request.StudentId.HasValue) q = q.Where(r => r.StudentId == request.StudentId.Value);
        if (request.Status.HasValue) q = q.Where(r => r.Status == request.Status.Value);

        var total = await q.CountAsync(ct);
        var rows = await q.OrderByDescending(r => r.Id)
            .Skip((page - 1) * size).Take(size).ToListAsync(ct);

        return Result.Success(new PagedResponse<RefundResponse>
        {
            Data = rows.Select(RefundResponse.FromEntity).ToList(),
            Page = page, Size = size, TotalCount = total,
            //TotalPages = (int)Math.Ceiling(total / (double)size),
        });
    }
}
