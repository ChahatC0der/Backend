using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Fees.Builders;
using SchoolERP.Application.Features.Transport.DTOs.RouteVehicle;
using SchoolERP.Domain.Shared.Results;
using RouteEntity = SchoolERP.Domain.Transport.Entities.Route;
using VehicleEntity = SchoolERP.Domain.Transport.Entities.Vehicle;
using RouteVehicleEntity = SchoolERP.Domain.Transport.Entities.RouteVehicle;

namespace SchoolERP.Application.Features.Transport.Queries.Assignment.GetRouteVehicleHistory;

public sealed class GetRouteVehicleHistoryQueryHandler : IRequestHandler<GetRouteVehicleHistoryQuery, Result<IReadOnlyList<RouteVehicleAssignmentResponse>>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;
    private readonly ICurrentBranchService _branch;

    public GetRouteVehicleHistoryQueryHandler(IApplicationDbContext db, ICurrentTenantService tenant, ICurrentBranchService branch)
    { _db = db; _tenant = tenant; _branch = branch; }

    public async Task<Result<IReadOnlyList<RouteVehicleAssignmentResponse>>> Handle(GetRouteVehicleHistoryQuery request, CancellationToken ct)
    {
        var scope = TransportScopeGuard.RequireBranch(_tenant, _branch);
        if (scope.IsFailure) return scope.Error!;
        var (_, branchId) = scope.Value;

        var query = _db.Set<RouteVehicleEntity>().AsNoTracking()
            .Where(rv => !rv.IsDeleted);

        if (request.VehicleId.HasValue) query = query.Where(rv => rv.VehicleId == request.VehicleId.Value);
        if (request.RouteId.HasValue) query = query.Where(rv => rv.RouteId == request.RouteId.Value);

        var rows = await query.OrderByDescending(rv => rv.ValidFrom).ThenByDescending(rv => rv.Id)
            .Take(500).ToListAsync(ct);

        // Filter by branch via route membership
        var routeIds = rows.Select(r => r.RouteId).Distinct().ToList();
        var allowedRouteIds = await _db.Set<RouteEntity>().AsNoTracking()
            .Where(r => routeIds.Contains(r.Id) && r.BranchId == branchId && !r.IsDeleted)
            .Select(r => r.Id).ToListAsync(ct);
        var allowedSet = allowedRouteIds.ToHashSet();
        rows = rows.Where(rv => allowedSet.Contains(rv.RouteId)).ToList();

        var routeLookup = await _db.Set<RouteEntity>().AsNoTracking()
            .Where(r => routeIds.Contains(r.Id))
            .ToDictionaryAsync(r => r.Id, r => new { r.Name, r.Code }, ct);

        var vehicleIds = rows.Select(r => r.VehicleId).Distinct().ToList();
        var vehicleLookup = await _db.Set<VehicleEntity>().AsNoTracking()
            .Where(v => vehicleIds.Contains(v.Id))
            .ToDictionaryAsync(v => v.Id, v => v.VehicleNumber, ct);

        var result = rows.Select(rv => new RouteVehicleAssignmentResponse
        {
            Id = rv.Id,
            RouteId = rv.RouteId,
            RouteName = routeLookup.TryGetValue(rv.RouteId, out var rn) ? rn.Name : null,
            RouteCode = routeLookup.TryGetValue(rv.RouteId, out var rc) ? rc.Code : null,
            VehicleId = rv.VehicleId,
            VehicleNumber = vehicleLookup.TryGetValue(rv.VehicleId, out var vn) ? vn : null,
            ValidFrom = rv.ValidFrom,
            ValidTo = rv.ValidTo,
            IsCurrent = rv.IsCurrent,
            CreatedAt = rv.CreatedAt,
        }).ToList();

        return Result.Success<IReadOnlyList<RouteVehicleAssignmentResponse>>(result);
    }
}
