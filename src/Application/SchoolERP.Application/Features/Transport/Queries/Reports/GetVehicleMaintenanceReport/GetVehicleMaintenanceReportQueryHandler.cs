using MediatR;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Transport.DTOs.Reports;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Transport.Queries.Reports.GetVehicleMaintenanceReport;

public sealed class GetVehicleMaintenanceReportQueryHandler : IRequestHandler<GetVehicleMaintenanceReportQuery, Result<IReadOnlyList<VehicleMaintenanceReportRow>>>
{
    private readonly IDapperRepository<VehicleMaintenanceReportRow> _repo;
    private readonly ICurrentTenantService _tenant;
    private readonly ICurrentBranchService _branch;

    public GetVehicleMaintenanceReportQueryHandler(
        IDapperRepository<VehicleMaintenanceReportRow> repo,
        ICurrentTenantService tenant, ICurrentBranchService branch)
    { _repo = repo; _tenant = tenant; _branch = branch; }

    public async Task<Result<IReadOnlyList<VehicleMaintenanceReportRow>>> Handle(GetVehicleMaintenanceReportQuery request, CancellationToken ct)
    {
        var tenantId = _tenant.GetTenantId();
        if (tenantId == Guid.Empty) return Error.Unauthorized("Tenant context is not resolved.");
        var branchId = _branch.GetBranchId();

        const string sql = @"
SELECT v.Id AS VehicleId, v.VehicleNumber, m.MaintenanceDate, m.Description, m.Cost, m.OdometerReading
FROM VehicleMaintenance m
INNER JOIN Vehicles v ON v.Id = m.VehicleId AND v.IsDeleted = 0
INNER JOIN Branches b ON b.Id = v.BranchId
WHERE b.TenantId = @t AND (@b IS NULL OR v.BranchId = @b) AND m.IsDeleted = 0
  AND (@vid IS NULL OR v.Id = @vid)
  AND (@from IS NULL OR m.MaintenanceDate >= CAST(@from AS DATE))
  AND (@to IS NULL OR m.MaintenanceDate <= CAST(@to AS DATE))
ORDER BY m.MaintenanceDate DESC, v.VehicleNumber;";

        var rows = await _repo.ExecuteRawSqlAsync(sql,
            new { t = tenantId, b = branchId, vid = request.VehicleId, from = request.From, to = request.To }, ct);
        return Result.Success<IReadOnlyList<VehicleMaintenanceReportRow>>(rows.ToList());
    }
}
