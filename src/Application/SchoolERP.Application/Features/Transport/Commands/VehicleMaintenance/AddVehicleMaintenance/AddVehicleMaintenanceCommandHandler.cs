using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Fees.Builders;
using SchoolERP.Application.Features.Transport.DTOs.VehicleMaintenance;
using SchoolERP.Domain.Shared.Results;
using VehicleEntity = SchoolERP.Domain.Transport.Entities.Vehicle;
using VehicleMaintenanceEntity = SchoolERP.Domain.Transport.Entities.VehicleMaintenance;

namespace SchoolERP.Application.Features.Transport.Commands.VehicleMaintenance.AddVehicleMaintenance;

public sealed class AddVehicleMaintenanceCommandHandler : IRequestHandler<AddVehicleMaintenanceCommand, Result<VehicleMaintenanceResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;
    private readonly ICurrentBranchService _branch;

    public AddVehicleMaintenanceCommandHandler(IApplicationDbContext db, ICurrentTenantService tenant, ICurrentBranchService branch)
    { _db = db; _tenant = tenant; _branch = branch; }

    public async Task<Result<VehicleMaintenanceResponse>> Handle(AddVehicleMaintenanceCommand request, CancellationToken ct)
    {
        var scope = TransportScopeGuard.RequireBranch(_tenant, _branch);
        if (scope.IsFailure) return scope.Error!;
        var (tenantId, branchId) = scope.Value;

        // Verify vehicle belongs to scope
        var vehicleExists = await _db.Set<VehicleEntity>().AnyAsync(v =>
            v.Id == request.VehicleId && v.TenantId == tenantId
            && v.BranchId == branchId && !v.IsDeleted, ct);
        if (!vehicleExists) return Error.NotFound("Vehicle", request.VehicleId.ToString());

        var entity = new VehicleMaintenanceEntity
        {
            VehicleId = request.VehicleId,
            MaintenanceDate = request.Request.MaintenanceDate,
            Description = request.Request.Description?.Trim(),
            Cost = request.Request.Cost,
            OdometerReading = request.Request.OdometerReading,
            CreatedAt = DateTime.UtcNow,
        };
        _db.Set<VehicleMaintenanceEntity>().Add(entity);
        await _db.SaveChangesAsync(ct);

        // TODO (audit): await _auditWriter.LogAsync("VehicleMaintenance", entity.Id, FinancialAuditAction.Create, null, entity, ct);

        return Result.Success(VehicleMaintenanceResponse.FromEntity(entity));
    }
}
