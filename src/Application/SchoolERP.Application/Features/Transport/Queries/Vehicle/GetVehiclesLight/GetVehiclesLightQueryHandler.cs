using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Fees.Builders;
using SchoolERP.Application.Features.Transport.DTOs.Vehicle;
using SchoolERP.Domain.Shared.Results;
using SchoolERP.Domain.Transport.Enums;
using VehicleEntity = SchoolERP.Domain.Transport.Entities.Vehicle;

namespace SchoolERP.Application.Features.Transport.Queries.Vehicle.GetVehiclesLight;

public sealed class GetVehiclesLightQueryHandler : IRequestHandler<GetVehiclesLightQuery, Result<IReadOnlyList<VehicleLightResponse>>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;
    private readonly ICurrentBranchService _branch;

    public GetVehiclesLightQueryHandler(IApplicationDbContext db, ICurrentTenantService tenant, ICurrentBranchService branch)
    { _db = db; _tenant = tenant; _branch = branch; }

    public async Task<Result<IReadOnlyList<VehicleLightResponse>>> Handle(GetVehiclesLightQuery request, CancellationToken ct)
    {
        var scope = TransportScopeGuard.RequireBranch(_tenant, _branch);
        if (scope.IsFailure) return scope.Error!;
        var (tenantId, branchId) = scope.Value;

        var rows = await _db.Set<VehicleEntity>().AsNoTracking()
            .Where(v => v.TenantId == tenantId && v.BranchId == branchId
                     && !v.IsDeleted && v.Status == VehicleStatus.Active)
            .OrderBy(v => v.VehicleNumber)
            .Select(v => new VehicleLightResponse
            {
                Id = v.Id,
                VehicleNumber = v.VehicleNumber,
                VehicleType = v.VehicleType.ToString(),
                Capacity = v.Capacity,
                Status = v.Status.ToString(),
            })
            .ToListAsync(ct);

        return Result.Success<IReadOnlyList<VehicleLightResponse>>(rows);
    }
}
