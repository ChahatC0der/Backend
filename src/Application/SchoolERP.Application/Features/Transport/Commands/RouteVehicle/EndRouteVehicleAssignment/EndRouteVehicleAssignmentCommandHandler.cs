using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Transport.DTOs.RouteVehicle;
using SchoolERP.Domain.Shared.Results;
using RouteEntity = SchoolERP.Domain.Transport.Entities.Route;
using VehicleEntity = SchoolERP.Domain.Transport.Entities.Vehicle;
using RouteVehicleEntity = SchoolERP.Domain.Transport.Entities.RouteVehicle;

namespace SchoolERP.Application.Features.Transport.Commands.RouteVehicle.EndRouteVehicleAssignment;

public sealed class EndRouteVehicleAssignmentCommandHandler : IRequestHandler<EndRouteVehicleAssignmentCommand, Result<RouteVehicleAssignmentResponse>>
{
    private readonly SchoolERP.Application.Common.Interfaces.IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;
    private readonly ICurrentBranchService _branch;

    public EndRouteVehicleAssignmentCommandHandler(SchoolERP.Application.Common.Interfaces.IApplicationDbContext db, ICurrentTenantService tenant, ICurrentBranchService branch)
    { _db = db; _tenant = tenant; _branch = branch; }

    public async Task<Result<RouteVehicleAssignmentResponse>> Handle(EndRouteVehicleAssignmentCommand request, CancellationToken ct)
    {
        var scope = TransportScopeGuard.RequireBranch(_tenant, _branch);
        if (scope.IsFailure) return scope.Error!;
        var (_, branchId) = scope.Value;

        var entity = await _db.Set<RouteVehicleEntity>().FirstOrDefaultAsync(rv =>
            rv.Id == request.AssignmentId && rv.IsCurrent && !rv.IsDeleted, ct);
        if (entity is null) return Error.NotFound("Current RouteVehicle assignment", request.AssignmentId.ToString());

        var vehicleOk = await _db.Set<VehicleEntity>().AsNoTracking()
            .AnyAsync(v => v.Id == entity.VehicleId && v.BranchId == branchId && !v.IsDeleted, ct);
        var routeOk = await _db.Set<RouteEntity>().AsNoTracking()
            .AnyAsync(r => r.Id == entity.RouteId && r.BranchId == branchId && !r.IsDeleted, ct);
        if (!vehicleOk || !routeOk) return Error.NotFound("RouteVehicle assignment", request.AssignmentId.ToString());

        var endDate = request.Request.EndDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
        if (endDate < entity.ValidFrom)
            return Error.Validation($"End date cannot be before ValidFrom ({entity.ValidFrom:yyyy-MM-dd}).");

        entity.IsCurrent = false;
        entity.ValidTo = endDate;
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);

        var route = await _db.Set<RouteEntity>().AsNoTracking().Where(r => r.Id == entity.RouteId)
            .Select(r => new { r.Name, r.Code }).FirstOrDefaultAsync(ct);
        var vehicle = await _db.Set<VehicleEntity>().AsNoTracking().Where(v => v.Id == entity.VehicleId)
            .Select(v => v.VehicleNumber).FirstOrDefaultAsync(ct);

        return Result.Success(new RouteVehicleAssignmentResponse
        {
            Id = entity.Id, RouteId = entity.RouteId,
            RouteName = route?.Name, RouteCode = route?.Code,
            VehicleId = entity.VehicleId, VehicleNumber = vehicle,
            ValidFrom = entity.ValidFrom, ValidTo = entity.ValidTo,
            IsCurrent = entity.IsCurrent, CreatedAt = entity.CreatedAt,
        });
    }
}
