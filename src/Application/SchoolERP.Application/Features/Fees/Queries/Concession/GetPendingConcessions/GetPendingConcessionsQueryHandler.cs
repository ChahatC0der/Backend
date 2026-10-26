using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.DTOs;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Fees.DTOs.Concession;
using SchoolERP.Domain.Fees.Enums;
using SchoolERP.Domain.Shared.Results;
using ConcessionEntity = SchoolERP.Domain.Fees.Entities.Concession;
using ConcessionAllocationEntity = SchoolERP.Domain.Fees.Entities.ConcessionAllocation;

namespace SchoolERP.Application.Features.Fees.Queries.Concession.GetPendingConcessions;

public sealed class GetPendingConcessionsQueryHandler : IRequestHandler<GetPendingConcessionsQuery, Result<PagedResponse<ConcessionAllocationResponse>>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;
    private readonly ICurrentBranchService _branch;

    public GetPendingConcessionsQueryHandler(IApplicationDbContext db, ICurrentTenantService tenant, ICurrentBranchService branch)
    { _db = db; _tenant = tenant; _branch = branch; }

    public async Task<Result<PagedResponse<ConcessionAllocationResponse>>> Handle(GetPendingConcessionsQuery request, CancellationToken ct)
    {
        var scope = FeeScopeGuard.RequireBranch(_tenant, _branch);
        if (scope.IsFailure) return scope.Error!;
        var (tenantId, branchId) = scope.Value;

        var page = request.Page <= 0 ? 1 : request.Page;
        var size = request.Size <= 0 ? 10 : Math.Min(request.Size, 200);

        var query =
            from a in _db.Set<ConcessionAllocationEntity>().AsNoTracking()
            join c in _db.Set<ConcessionEntity>().AsNoTracking() on a.ConcessionId equals c.Id
            where a.Status == ConcessionAllocationStatus.Pending
               && c.TenantId == tenantId && c.BranchId == branchId && c.DeletedAt == null
            select new { a, c };

        var total = await query.CountAsync(ct);
        var rows = await query.OrderBy(x => x.a.CreatedAt)
            .Skip((page - 1) * size).Take(size).ToListAsync(ct);

        return Result.Success(new PagedResponse<ConcessionAllocationResponse>
        {
            Data = rows.Select(x => new ConcessionAllocationResponse
            {
                Id = x.a.Id, ConcessionId = x.a.ConcessionId, ConcessionName = x.c.Name,
                StudentId = x.a.StudentId, AcademicYearId = x.a.AcademicYearId,
                Status = x.a.Status.ToString(),
                ApprovedBy = x.a.ApprovedBy, ApprovedAt = x.a.ApprovedAt,
                RejectionReason = x.a.RejectionReason,
                CreatedAt = x.a.CreatedAt, UpdatedAt = x.a.UpdatedAt,
            }).ToList(),
            Page = page, Size = size, TotalCount = total,
            //TotalPages = (int)Math.Ceiling(total / (double)size),
        });
    }
}
