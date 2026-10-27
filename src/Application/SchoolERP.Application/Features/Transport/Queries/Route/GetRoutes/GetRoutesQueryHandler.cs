using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.DTOs;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Fees.Builders;
using SchoolERP.Application.Features.Transport.DTOs.Route;
using SchoolERP.Domain.Shared.Results;
using RouteEntity = SchoolERP.Domain.Transport.Entities.Route;
using StopEntity = SchoolERP.Domain.Transport.Entities.Stop;

namespace SchoolERP.Application.Features.Transport.Queries.Route.GetRoutes;

public sealed class GetRoutesQueryHandler : IRequestHandler<GetRoutesQuery, Result<PagedResponse<RouteResponse>>>
{
    private static readonly HashSet<string> AllowedSort = new(StringComparer.OrdinalIgnoreCase)
        { "Name", "Code", "Status", "CreatedAt" };

    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;
    private readonly ICurrentBranchService _branch;

    public GetRoutesQueryHandler(IApplicationDbContext db, ICurrentTenantService tenant, ICurrentBranchService branch)
    { _db = db; _tenant = tenant; _branch = branch; }

    public async Task<Result<PagedResponse<RouteResponse>>> Handle(GetRoutesQuery request, CancellationToken ct)
    {
        var scope = TransportScopeGuard.RequireBranch(_tenant, _branch);
        if (scope.IsFailure) return scope.Error!;
        var (_, branchId) = scope.Value;

        var page = request.Page <= 0 ? 1 : request.Page;
        var size = request.Size <= 0 ? 10 : Math.Min(request.Size, 200);

        var query = _db.Set<RouteEntity>().AsNoTracking()
            .Where(r => r.BranchId == branchId && !r.IsDeleted);

        if (request.Status.HasValue) query = query.Where(r => r.Status == request.Status.Value);
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.Trim();
            query = query.Where(r => r.Name.Contains(term) || r.Code.Contains(term));
        }

        var sortBy = AllowedSort.Contains(request.SortBy ?? "") ? request.SortBy! : "Name";
        var desc = string.Equals(request.SortOrder, "desc", StringComparison.OrdinalIgnoreCase);
        query = sortBy switch
        {
            "Code"      => desc ? query.OrderByDescending(r => r.Code)      : query.OrderBy(r => r.Code),
            "Status"    => desc ? query.OrderByDescending(r => r.Status)    : query.OrderBy(r => r.Status),
            "CreatedAt" => desc ? query.OrderByDescending(r => r.CreatedAt) : query.OrderBy(r => r.CreatedAt),
            _           => desc ? query.OrderByDescending(r => r.Name)      : query.OrderBy(r => r.Name),
        };

        var total = await query.CountAsync(ct);
        var rows = await query.Skip((page - 1) * size).Take(size).ToListAsync(ct);

        var ids = rows.Select(r => r.Id).ToList();
        var stopCounts = await _db.Set<StopEntity>().AsNoTracking()
            .Where(s => ids.Contains(s.RouteId) && !s.IsDeleted)
            .GroupBy(s => s.RouteId)
            .Select(g => new { RouteId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.RouteId, x => x.Count, ct);

        return Result.Success(new PagedResponse<RouteResponse>
        {
            Data = rows.Select(r => RouteResponse.FromEntity(r, stopCounts.TryGetValue(r.Id, out var c) ? c : 0)).ToList(),
            Page = page, Size = size, TotalCount = total,
            //TotalPages = (int)Math.Ceiling(total / (double)size),
        });
    }
}
