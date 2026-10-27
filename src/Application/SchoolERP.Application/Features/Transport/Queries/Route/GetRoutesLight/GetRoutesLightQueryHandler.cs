using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Fees.Builders;
using SchoolERP.Application.Features.Transport.DTOs.Route;
using SchoolERP.Domain.Shared.Results;
using SchoolERP.Domain.Transport.Enums;
using RouteEntity = SchoolERP.Domain.Transport.Entities.Route;

namespace SchoolERP.Application.Features.Transport.Queries.Route.GetRoutesLight;

public sealed class GetRoutesLightQueryHandler : IRequestHandler<GetRoutesLightQuery, Result<IReadOnlyList<RouteLightResponse>>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;
    private readonly ICurrentBranchService _branch;

    public GetRoutesLightQueryHandler(IApplicationDbContext db, ICurrentTenantService tenant, ICurrentBranchService branch)
    { _db = db; _tenant = tenant; _branch = branch; }

    public async Task<Result<IReadOnlyList<RouteLightResponse>>> Handle(GetRoutesLightQuery request, CancellationToken ct)
    {
        var scope = TransportScopeGuard.RequireBranch(_tenant, _branch);
        if (scope.IsFailure) return scope.Error!;
        var (_, branchId) = scope.Value;

        var rows = await _db.Set<RouteEntity>().AsNoTracking()
            .Where(r => r.BranchId == branchId && !r.IsDeleted && r.Status == RouteStatus.Active)
            .OrderBy(r => r.Name)
            .Select(r => new RouteLightResponse { Id = r.Id, Name = r.Name, Code = r.Code })
            .ToListAsync(ct);

        return Result.Success<IReadOnlyList<RouteLightResponse>>(rows);
    }
}
