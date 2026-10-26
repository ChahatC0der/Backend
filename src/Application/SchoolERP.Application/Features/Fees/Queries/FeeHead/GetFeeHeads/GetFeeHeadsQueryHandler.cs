using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.DTOs;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Fees.DTOs.FeeHead;
using FeeHeadEntity = SchoolERP.Domain.Fees.Entities.FeeHead;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Fees.Queries.FeeHead.GetFeeHeads;

public sealed class GetFeeHeadsQueryHandler : IRequestHandler<GetFeeHeadsQuery, Result<PagedResponse<FeeHeadResponse>>>
{
    private static readonly HashSet<string> AllowedSort = new(StringComparer.OrdinalIgnoreCase)
        { "Name", "Code", "Status", "CreatedAt" };

    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;
    private readonly ICurrentBranchService _branch;

    public GetFeeHeadsQueryHandler(IApplicationDbContext db, ICurrentTenantService tenant, ICurrentBranchService branch)
    {
        _db = db; _tenant = tenant; _branch = branch;
    }

    public async Task<Result<PagedResponse<FeeHeadResponse>>> Handle(GetFeeHeadsQuery request, CancellationToken ct)
    {
        var scope = FeeScopeGuard.RequireBranch(_tenant, _branch);
        if (scope.IsFailure) return scope.Error!;
        var (tenantId, branchId) = scope.Value;

        var page = request.Page <= 0 ? 1 : request.Page;
        var size = request.Size <= 0 ? 10 : Math.Min(request.Size, 200);

        var query = _db.Set<FeeHeadEntity>().AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.BranchId == branchId && x.DeletedAt == null);

        if (request.Status.HasValue) query = query.Where(x => x.Status == request.Status.Value);
        if (request.IsOptional.HasValue) query = query.Where(x => x.IsOptional == request.IsOptional.Value);
        if (request.IsMandatory.HasValue) query = query.Where(x => x.IsMandatory == request.IsMandatory.Value);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.Trim();
            query = query.Where(x => x.Name.Contains(term) || x.Code.Contains(term));
        }

        var sortBy = AllowedSort.Contains(request.SortBy ?? "") ? request.SortBy! : "Name";
        var desc = string.Equals(request.SortOrder, "desc", StringComparison.OrdinalIgnoreCase);
        query = sortBy switch
        {
            "Code"      => desc ? query.OrderByDescending(x => x.Code)      : query.OrderBy(x => x.Code),
            "Status"    => desc ? query.OrderByDescending(x => x.Status)    : query.OrderBy(x => x.Status),
            "CreatedAt" => desc ? query.OrderByDescending(x => x.CreatedAt) : query.OrderBy(x => x.CreatedAt),
            _           => desc ? query.OrderByDescending(x => x.Name)      : query.OrderBy(x => x.Name),
        };

        var total = await query.CountAsync(ct);
        var rows = await query.Skip((page - 1) * size).Take(size).ToListAsync(ct);

        return Result.Success(new PagedResponse<FeeHeadResponse>
        {
            Data = rows.Select(FeeHeadResponse.FromEntity).ToList(),
            Page = page,
            Size = size,
            TotalCount = total,
            //TotalPages = (int)Math.Ceiling(total / (double)size),
        });
    }
}
