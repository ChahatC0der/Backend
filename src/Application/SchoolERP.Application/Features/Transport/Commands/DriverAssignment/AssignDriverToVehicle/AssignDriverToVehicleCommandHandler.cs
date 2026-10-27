using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Transport.DTOs.DriverAssignment;
using SchoolERP.Domain.Shared.Results;
using SchoolERP.Domain.Transport.Enums;
using VehicleEntity = SchoolERP.Domain.Transport.Entities.Vehicle;
using DriverAssignmentEntity = SchoolERP.Domain.Transport.Entities.DriverAssignment;

namespace SchoolERP.Application.Features.Transport.Commands.DriverAssignment.AssignDriverToVehicle;

public sealed class AssignDriverToVehicleCommandHandler : IRequestHandler<AssignDriverToVehicleCommand, Result<DriverAssignmentResponse>>
{
    private readonly SchoolERP.Application.Common.Interfaces.IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;
    private readonly ICurrentBranchService _branch;

    public AssignDriverToVehicleCommandHandler(SchoolERP.Application.Common.Interfaces.IApplicationDbContext db, ICurrentTenantService tenant, ICurrentBranchService branch)
    { _db = db; _tenant = tenant; _branch = branch; }

    public async Task<Result<DriverAssignmentResponse>> Handle(AssignDriverToVehicleCommand request, CancellationToken ct)
    {
        var scope = TransportScopeGuard.RequireBranch(_tenant, _branch);
        if (scope.IsFailure) return scope.Error!;
        var (tenantId, branchId) = scope.Value;

        var vehicle = await _db.Set<VehicleEntity>().AsNoTracking().FirstOrDefaultAsync(v =>
            v.Id == request.VehicleId && v.TenantId == tenantId
            && v.BranchId == branchId && !v.IsDeleted, ct);
        if (vehicle is null) return Error.NotFound("Vehicle", request.VehicleId.ToString());
        if (vehicle.Status != VehicleStatus.Active)
            return Error.Conflict($"Vehicle is {vehicle.Status}. Only Active vehicles can have a driver.");

        // TODO (Phase 3.1): validate DriverStaffId role via Staff lookup.

        var current = await _db.Set<DriverAssignmentEntity>().FirstOrDefaultAsync(da =>
            da.VehicleId == vehicle.Id && da.IsCurrent && !da.IsDeleted, ct);

        if (current is not null)
        {
            if (current.DriverStaffId == request.Request.DriverStaffId)
                return Error.Conflict($"Driver already assigned to this vehicle since {current.ValidFrom:yyyy-MM-dd}.");
            if (request.Request.ValidFrom <= current.ValidFrom)
                return Error.Validation($"New ValidFrom must be after current ({current.ValidFrom:yyyy-MM-dd}).");
            current.IsCurrent = false;
            current.ValidTo = request.Request.ValidFrom.AddDays(-1);
            current.UpdatedAt = DateTime.UtcNow;
        }

        var entity = new DriverAssignmentEntity
        {
            VehicleId = vehicle.Id,
            DriverStaffId = request.Request.DriverStaffId,
            ValidFrom = request.Request.ValidFrom,
            ValidTo = null,
            IsCurrent = true,
            CreatedAt = DateTime.UtcNow,
        };
        _db.Set<DriverAssignmentEntity>().Add(entity);
        await _db.SaveChangesAsync(ct);

        return Result.Success(new DriverAssignmentResponse
        {
            Id = entity.Id, VehicleId = vehicle.Id, VehicleNumber = vehicle.VehicleNumber,
            DriverStaffId = entity.DriverStaffId, DriverName = null,
            ValidFrom = entity.ValidFrom, ValidTo = entity.ValidTo,
            IsCurrent = entity.IsCurrent, CreatedAt = entity.CreatedAt,
        });
    }
}
