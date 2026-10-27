using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Transport.DTOs.RouteVehicle;
using SchoolERP.Domain.Shared.Results;
using SchoolERP.Domain.Transport.Enums;
using RouteEntity = SchoolERP.Domain.Transport.Entities.Route;
using VehicleEntity = SchoolERP.Domain.Transport.Entities.Vehicle;
using RouteVehicleEntity = SchoolERP.Domain.Transport.Entities.RouteVehicle;

namespace SchoolERP.Application.Features.Transport.Commands.RouteVehicle.AssignVehicleToRoute;

public sealed class AssignVehicleToRouteCommandHandler : IRequestHandler<AssignVehicleToRouteCommand, Result<RouteVehicleAssignmentResponse>>
{
    private readonly SchoolERP.Application.Common.Interfaces.IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;
    private readonly ICurrentBranchService _branch;

    public AssignVehicleToRouteCommandHandler(SchoolERP.Application.Common.Interfaces.IApplicationDbContext db, ICurrentTenantService tenant, ICurrentBranchService branch)
    { _db = db; _tenant = tenant; _branch = branch; }

    public async Task<Result<RouteVehicleAssignmentResponse>> Handle(AssignVehicleToRouteCommand request, CancellationToken ct)
    {
        var scope = TransportScopeGuard.RequireBranch(_tenant, _branch);
        if (scope.IsFailure) return scope.Error!;
        var (_, branchId) = scope.Value;

        var route = await _db.Set<RouteEntity>().AsNoTracking().FirstOrDefaultAsync(r =>
            r.Id == request.RouteId && r.BranchId == branchId && !r.IsDeleted, ct);
        if (route is null) return Error.NotFound("Route", request.RouteId.ToString());
        if (route.Status == RouteStatus.Inactive)
            return Error.Conflict("Inactive routes cannot receive new vehicle assignments.");

        var vehicle = await _db.Set<VehicleEntity>().AsNoTracking().FirstOrDefaultAsync(v =>
            v.Id == request.Request.VehicleId && v.BranchId == branchId && !v.IsDeleted, ct);
        if (vehicle is null) return Error.NotFound("Vehicle", request.Request.VehicleId.ToString());
        if (vehicle.Status != VehicleStatus.Active)
            return Error.Conflict($"Vehicle is {vehicle.Status}. Only Active vehicles can be assigned.");

        var current = await _db.Set<RouteVehicleEntity>().FirstOrDefaultAsync(rv =>
            rv.VehicleId == vehicle.Id && rv.IsCurrent && !rv.IsDeleted, ct);

        if (current is not null)
        {
            if (current.RouteId == route.Id)
                return Error.Conflict($"Vehicle already on route {route.Code} since {current.ValidFrom:yyyy-MM-dd}.");
            if (request.Request.ValidFrom <= current.ValidFrom)
                return Error.Validation($"New ValidFrom must be after current assignment ({current.ValidFrom:yyyy-MM-dd}).");
            current.IsCurrent = false;
            current.ValidTo = request.Request.ValidFrom.AddDays(-1);
            current.UpdatedAt = DateTime.UtcNow;
        }

        var entity = new RouteVehicleEntity
        {
            RouteId = route.Id, VehicleId = vehicle.Id,
            ValidFrom = request.Request.ValidFrom, ValidTo = null,
            IsCurrent = true, CreatedAt = DateTime.UtcNow,
        };
        _db.Set<RouteVehicleEntity>().Add(entity);
        await _db.SaveChangesAsync(ct);

        return Result.Success(new RouteVehicleAssignmentResponse
        {
            Id = entity.Id, RouteId = route.Id, RouteName = route.Name, RouteCode = route.Code,
            VehicleId = vehicle.Id, VehicleNumber = vehicle.VehicleNumber,
            ValidFrom = entity.ValidFrom, ValidTo = entity.ValidTo,
            IsCurrent = entity.IsCurrent, CreatedAt = entity.CreatedAt,
        });
    }
}
