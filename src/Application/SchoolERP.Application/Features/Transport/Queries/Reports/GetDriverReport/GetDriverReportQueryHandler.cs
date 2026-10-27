using MediatR;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Transport.DTOs.Reports;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Transport.Queries.Reports.GetDriverReport;

public sealed class GetDriverReportQueryHandler : IRequestHandler<GetDriverReportQuery, Result<IReadOnlyList<DriverReportRow>>>
{
    private readonly IDapperRepository<DriverReportRow> _repo;
    private readonly ICurrentTenantService _tenant;
    private readonly ICurrentBranchService _branch;

    public GetDriverReportQueryHandler(
        IDapperRepository<DriverReportRow> repo,
        ICurrentTenantService tenant, ICurrentBranchService branch)
    { _repo = repo; _tenant = tenant; _branch = branch; }

    public async Task<Result<IReadOnlyList<DriverReportRow>>> Handle(GetDriverReportQuery request, CancellationToken ct)
    {
        var tenantId = _tenant.GetTenantId();
        if (tenantId == Guid.Empty) return Error.Unauthorized("Tenant context is not resolved.");
        var branchId = _branch.GetBranchId();

        const string sql = @"
SELECT da.VehicleId, v.VehicleNumber, da.DriverStaffId, da.ValidFrom, da.ValidTo, da.IsCurrent
FROM DriverAssignments da
INNER JOIN Vehicles v ON v.Id = da.VehicleId AND v.IsDeleted = 0
INNER JOIN Branches b ON b.Id = v.BranchId
WHERE b.TenantId = @t AND (@b IS NULL OR v.BranchId = @b) AND da.IsDeleted = 0
  AND (@current IS NULL OR da.IsCurrent = @current)
ORDER BY v.VehicleNumber, da.ValidFrom DESC;";

        var rows = await _repo.ExecuteRawSqlAsync(sql,
            new { t = tenantId, b = branchId, current = request.OnlyCurrent }, ct);
        return Result.Success<IReadOnlyList<DriverReportRow>>(rows.ToList());
    }
}
