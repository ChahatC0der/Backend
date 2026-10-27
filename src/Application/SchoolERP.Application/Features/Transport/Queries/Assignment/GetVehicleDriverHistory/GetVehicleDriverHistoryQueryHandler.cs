using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Fees.Builders;
using SchoolERP.Application.Features.Transport.DTOs.DriverAssignment;
using SchoolERP.Domain.Shared.Results;
using VehicleEntity = SchoolERP.Domain.Transport.Entities.Vehicle;
using DriverAssignmentEntity = SchoolERP.Domain.Transport.Entities.DriverAssignment;

namespace SchoolERP.Application.Features.Transport.Queries.Assignment.GetVehicleDriverHistory;

public sealed class GetVehicleDriverHistoryQueryHandler : IRequestHandler<GetVehicleDriverHistoryQuery, Result<IReadOnlyList<DriverAssignmentResponse>>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;
    private readonly ICurrentBranchService _branch;

    public GetVehicleDriverHistoryQueryHandler(IApplicationDbContext db, ICurrentTenantService tenant, ICurrentBranchService branch)
    { _db = db; _tenant = tenant; _branch = branch; }

    public async Task<Result<IReadOnlyList<DriverAssignmentResponse>>> Handle(GetVehicleDriverHistoryQuery request, CancellationToken ct)
    {
        var scope = TransportScopeGuard.RequireBranch(_tenant, _branch);
        if (scope.IsFailure) return scope.Error!;
        var (_, branchId) = scope.Value;

        var query = _db.Set<DriverAssignmentEntity>().AsNoTracking()
            .Where(da => !da.IsDeleted);

        if (request.VehicleId.HasValue) query = query.Where(da => da.VehicleId == request.VehicleId.Value);
        if (request.DriverStaffId.HasValue) query = query.Where(da => da.DriverStaffId == request.DriverStaffId.Value);

        var rows = await query.OrderByDescending(da => da.ValidFrom).ThenByDescending(da => da.Id)
            .Take(500).ToListAsync(ct);

        // Branch filter via vehicles
        var vehicleIds = rows.Select(r => r.VehicleId).Distinct().ToList();
        var allowedVehicleIds = await _db.Set<VehicleEntity>().AsNoTracking()
            .Where(v => vehicleIds.Contains(v.Id) && v.BranchId == branchId && !v.IsDeleted)
            .Select(v => v.Id).ToListAsync(ct);
        var allowedSet = allowedVehicleIds.ToHashSet();
        rows = rows.Where(da => allowedSet.Contains(da.VehicleId)).ToList();

        var vehicleLookup = await _db.Set<VehicleEntity>().AsNoTracking()
            .Where(v => vehicleIds.Contains(v.Id))
            .ToDictionaryAsync(v => v.Id, v => v.VehicleNumber, ct);

        var result = rows.Select(da => new DriverAssignmentResponse
        {
            Id = da.Id,
            VehicleId = da.VehicleId,
            VehicleNumber = vehicleLookup.TryGetValue(da.VehicleId, out var vn) ? vn : null,
            DriverStaffId = da.DriverStaffId,
            DriverName = null,
            ValidFrom = da.ValidFrom,
            ValidTo = da.ValidTo,
            IsCurrent = da.IsCurrent,
            CreatedAt = da.CreatedAt,
        }).ToList();

        return Result.Success<IReadOnlyList<DriverAssignmentResponse>>(result);
    }
}
