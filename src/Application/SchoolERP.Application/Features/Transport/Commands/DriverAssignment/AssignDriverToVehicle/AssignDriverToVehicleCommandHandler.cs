
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Transport.DTOs.DriverAssignment;
using SchoolERP.Domain.Shared.Results;
using StaffEntity = SchoolERP.Domain.Staff.Entities.Staff;
using SchoolERP.Domain.Transport.Enums;

using VehicleEntity = SchoolERP.Domain.Transport.Entities.Vehicle;
using DriverAssignmentEntity = SchoolERP.Domain.Transport.Entities.DriverAssignment;

namespace SchoolERP.Application.Features.Transport.Commands.DriverAssignment.AssignDriverToVehicle;

public sealed class AssignDriverToVehicleCommandHandler
    : IRequestHandler<AssignDriverToVehicleCommand, Result<DriverAssignmentResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;
    private readonly ICurrentBranchService _branch;

    public AssignDriverToVehicleCommandHandler(
        IApplicationDbContext db,
        ICurrentTenantService tenant,
        ICurrentBranchService branch)
    {
        _db = db;
        _tenant = tenant;
        _branch = branch;
    }

    public async Task<Result<DriverAssignmentResponse>> Handle(
        AssignDriverToVehicleCommand request,
        CancellationToken ct)
    {
        // 1. Validate current tenant and branch scope.
        var scope = TransportScopeGuard.RequireBranch(_tenant, _branch);

        if (scope.IsFailure)
            return scope.Error!;

        var (tenantId, branchId) = scope.Value;

        // 2. Find vehicle within the current tenant and branch.
        var vehicle = await _db.Set<VehicleEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(v =>
                v.Id == request.VehicleId &&
                v.TenantId == tenantId &&
                v.BranchId == branchId &&
                !v.IsDeleted,
                ct);

        if (vehicle is null)
            return Error.NotFound("Vehicle", request.VehicleId.ToString());

        // 3. Only active vehicles can have a driver.
        if (vehicle.Status != VehicleStatus.Active)
        {
            return Error.Conflict(
                $"Vehicle is {vehicle.Status}. Only Active vehicles can have a driver.");
        }

        // 4. Validate driver Staff belongs to the same tenant and branch.
        var driver = await _db.Set<StaffEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(s =>
                s.Id == request.Request.DriverStaffId &&
                s.TenantId == tenantId &&
                s.BranchId == branchId &&
                !s.IsDeleted,
                ct);

        if (driver is null)
        {
            return Error.NotFound(
                "Driver staff",
                request.Request.DriverStaffId.ToString());
        }

        // 5. Check whether this vehicle already has a current driver.
        var current = await _db.Set<DriverAssignmentEntity>()
            .FirstOrDefaultAsync(da =>
                da.VehicleId == vehicle.Id &&
                da.IsCurrent,
                ct);

        if (current is not null)
        {
            // Same driver cannot be assigned again to the same vehicle.
            if (current.DriverStaffId == request.Request.DriverStaffId)
            {
                return Error.Conflict(
                    $"Driver already assigned to this vehicle since {current.ValidFrom:yyyy-MM-dd}.");
            }

            // New assignment must start after the current assignment.
            if (request.Request.ValidFrom <= current.ValidFrom)
            {
                return Error.Validation(
                    $"New ValidFrom must be after current ({current.ValidFrom:yyyy-MM-dd}).");
            }

            // End the previous assignment one day before the new assignment.
            current.IsCurrent = false;
            current.ValidTo = request.Request.ValidFrom.AddDays(-1);
            current.UpdatedAt = DateTime.UtcNow;
        }

        // 6. Create the new driver assignment.
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

        // 7. Save changes.
        await _db.SaveChangesAsync(ct);

        // 8. Return assignment response.
        return Result.Success(new DriverAssignmentResponse
        {
            Id = entity.Id,
            VehicleId = vehicle.Id,
            VehicleNumber = vehicle.VehicleNumber,
            DriverStaffId = entity.DriverStaffId,
            DriverName = null,
            ValidFrom = entity.ValidFrom,
            ValidTo = entity.ValidTo,
            IsCurrent = entity.IsCurrent,
            CreatedAt = entity.CreatedAt,
        });
    }
}
