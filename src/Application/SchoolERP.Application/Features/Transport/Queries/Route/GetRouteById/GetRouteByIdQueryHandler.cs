using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Fees.Builders;
using SchoolERP.Application.Features.Transport.DTOs.Route;
using SchoolERP.Domain.Shared.Results;
using RouteEntity = SchoolERP.Domain.Transport.Entities.Route;
using StopEntity = SchoolERP.Domain.Transport.Entities.Stop;

namespace SchoolERP.Application.Features.Transport.Queries.Route.GetRouteById;

public sealed class GetRouteByIdQueryHandler : IRequestHandler<GetRouteByIdQuery, Result<RouteResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;
    private readonly ICurrentBranchService _branch;

    public GetRouteByIdQueryHandler(IApplicationDbContext db, ICurrentTenantService tenant, ICurrentBranchService branch)
    { _db = db; _tenant = tenant; _branch = branch; }

    public async Task<Result<RouteResponse>> Handle(GetRouteByIdQuery request, CancellationToken ct)
    {
        var scope = TransportScopeGuard.RequireBranch(_tenant, _branch);
        if (scope.IsFailure) return scope.Error!;
        var (_, branchId) = scope.Value;

        var entity = await _db.Set<RouteEntity>().AsNoTracking().FirstOrDefaultAsync(r =>
            r.Id == request.Id && r.BranchId == branchId && !r.IsDeleted, ct);
        if (entity is null) return Error.NotFound("Route", request.Id.ToString());

        var stopCount = await _db.Set<StopEntity>().AsNoTracking()
            .CountAsync(s => s.RouteId == entity.Id && !s.IsDeleted, ct);

        return Result.Success(RouteResponse.FromEntity(entity, stopCount));
    }
}
