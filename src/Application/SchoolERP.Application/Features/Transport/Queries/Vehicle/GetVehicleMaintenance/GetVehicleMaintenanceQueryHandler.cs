using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Fees.Builders;
using SchoolERP.Application.Features.Transport.DTOs.VehicleMaintenance;
using SchoolERP.Domain.Shared.Results;
using VehicleEntity = SchoolERP.Domain.Transport.Entities.Vehicle;
using VehicleMaintenanceEntity = SchoolERP.Domain.Transport.Entities.VehicleMaintenance;

namespace SchoolERP.Application.Features.Transport.Queries.Vehicle.GetVehicleMaintenance;

public sealed class GetVehicleMaintenanceQueryHandler : IRequestHandler<GetVehicleMaintenanceQuery, Result<IReadOnlyList<VehicleMaintenanceResponse>>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;
    private readonly ICurrentBranchService _branch;

    public GetVehicleMaintenanceQueryHandler(IApplicationDbContext db, ICurrentTenantService tenant, ICurrentBranchService branch)
    { _db = db; _tenant = tenant; _branch = branch; }

    public async Task<Result<IReadOnlyList<VehicleMaintenanceResponse>>> Handle(GetVehicleMaintenanceQuery request, CancellationToken ct)
    {
        var scope = TransportScopeGuard.RequireBranch(_tenant, _branch);
        if (scope.IsFailure) return scope.Error!;
        var (tenantId, branchId) = scope.Value;

        var ok = await _db.Set<VehicleEntity>().AsNoTracking().AnyAsync(v =>
            v.Id == request.VehicleId && v.TenantId == tenantId
            && v.BranchId == branchId && !v.IsDeleted, ct);
        if (!ok) return Error.NotFound("Vehicle", request.VehicleId.ToString());

        var rows = await _db.Set<VehicleMaintenanceEntity>().AsNoTracking()
            .Where(m => m.VehicleId == request.VehicleId && !m.IsDeleted)
            .OrderByDescending(m => m.MaintenanceDate)
            .Take(500)
            .ToListAsync(ct);

        return Result.Success<IReadOnlyList<VehicleMaintenanceResponse>>(
            rows.Select(VehicleMaintenanceResponse.FromEntity).ToList());
    }
}
