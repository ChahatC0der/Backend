using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Transport.DTOs.DriverAssignment;
using SchoolERP.Domain.Shared.Results;
using VehicleEntity = SchoolERP.Domain.Transport.Entities.Vehicle;
using DriverAssignmentEntity = SchoolERP.Domain.Transport.Entities.DriverAssignment;

namespace SchoolERP.Application.Features.Transport.Commands.DriverAssignment.EndDriverAssignment;

public sealed class EndDriverAssignmentCommandHandler : IRequestHandler<EndDriverAssignmentCommand, Result<DriverAssignmentResponse>>
{
    private readonly SchoolERP.Application.Common.Interfaces.IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;
    private readonly ICurrentBranchService _branch;

    public EndDriverAssignmentCommandHandler(SchoolERP.Application.Common.Interfaces.IApplicationDbContext db, ICurrentTenantService tenant, ICurrentBranchService branch)
    { _db = db; _tenant = tenant; _branch = branch; }

    public async Task<Result<DriverAssignmentResponse>> Handle(EndDriverAssignmentCommand request, CancellationToken ct)
    {
        var scope = TransportScopeGuard.RequireBranch(_tenant, _branch);
        if (scope.IsFailure) return scope.Error!;
        var (_, branchId) = scope.Value;

        var entity = await _db.Set<DriverAssignmentEntity>().FirstOrDefaultAsync(da =>
            da.Id == request.AssignmentId && da.IsCurrent && !da.IsDeleted, ct);
        if (entity is null) return Error.NotFound("Current DriverAssignment", request.AssignmentId.ToString());

        var vehicleOk = await _db.Set<VehicleEntity>().AsNoTracking()
            .AnyAsync(v => v.Id == entity.VehicleId && v.BranchId == branchId && !v.IsDeleted, ct);
        if (!vehicleOk) return Error.NotFound("DriverAssignment", request.AssignmentId.ToString());

        var endDate = request.Request.EndDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
        if (endDate < entity.ValidFrom)
            return Error.Validation($"End date cannot be before ValidFrom ({entity.ValidFrom:yyyy-MM-dd}).");

        entity.IsCurrent = false;
        entity.ValidTo = endDate;
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);

        var vehicleNumber = await _db.Set<VehicleEntity>().AsNoTracking()
            .Where(v => v.Id == entity.VehicleId).Select(v => v.VehicleNumber).FirstOrDefaultAsync(ct);

        return Result.Success(new DriverAssignmentResponse
        {
            Id = entity.Id, VehicleId = entity.VehicleId, VehicleNumber = vehicleNumber,
            DriverStaffId = entity.DriverStaffId, DriverName = null,
            ValidFrom = entity.ValidFrom, ValidTo = entity.ValidTo,
            IsCurrent = entity.IsCurrent, CreatedAt = entity.CreatedAt,
        });
    }
}
