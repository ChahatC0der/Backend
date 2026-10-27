using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Fees.Builders;
using SchoolERP.Application.Features.Transport.DTOs.Stop;
using SchoolERP.Domain.Shared.Results;
using RouteEntity = SchoolERP.Domain.Transport.Entities.Route;
using StopEntity = SchoolERP.Domain.Transport.Entities.Stop;

namespace SchoolERP.Application.Features.Transport.Queries.Route.GetStopsByRoute;

public sealed class GetStopsByRouteQueryHandler : IRequestHandler<GetStopsByRouteQuery, Result<IReadOnlyList<StopResponse>>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;
    private readonly ICurrentBranchService _branch;

    public GetStopsByRouteQueryHandler(IApplicationDbContext db, ICurrentTenantService tenant, ICurrentBranchService branch)
    { _db = db; _tenant = tenant; _branch = branch; }

    public async Task<Result<IReadOnlyList<StopResponse>>> Handle(GetStopsByRouteQuery request, CancellationToken ct)
    {
        var scope = TransportScopeGuard.RequireBranch(_tenant, _branch);
        if (scope.IsFailure) return scope.Error!;
        var (_, branchId) = scope.Value;

        var routeOk = await _db.Set<RouteEntity>().AsNoTracking().AnyAsync(r =>
            r.Id == request.RouteId && r.BranchId == branchId && !r.IsDeleted, ct);
        if (!routeOk) return Error.NotFound("Route", request.RouteId.ToString());

        var rows = await _db.Set<StopEntity>().AsNoTracking()
            .Where(s => s.RouteId == request.RouteId && !s.IsDeleted)
            .OrderBy(s => s.StopOrder)
            .ToListAsync(ct);

        return Result.Success<IReadOnlyList<StopResponse>>(rows.Select(StopResponse.FromEntity).ToList());
    }
}
