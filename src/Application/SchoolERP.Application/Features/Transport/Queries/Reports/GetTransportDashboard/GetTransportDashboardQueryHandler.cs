using MediatR;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Transport.DTOs.Reports;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Transport.Queries.Reports.GetTransportDashboard;

/// <summary>Counts-based dashboard. Reads aggregates only. Tenant + branch scoped via join to Branches.</summary>
public sealed class GetTransportDashboardQueryHandler : IRequestHandler<GetTransportDashboardQuery, Result<TransportDashboardResponse>>
{
    private readonly IDapperRepository<TransportDashboardResponse> _repo;
    private readonly ICurrentTenantService _tenant;
    private readonly ICurrentBranchService _branch;

    public GetTransportDashboardQueryHandler(
        IDapperRepository<TransportDashboardResponse> repo,
        ICurrentTenantService tenant, ICurrentBranchService branch)
    { _repo = repo; _tenant = tenant; _branch = branch; }

    public async Task<Result<TransportDashboardResponse>> Handle(GetTransportDashboardQuery request, CancellationToken ct)
    {
        var tenantId = _tenant.GetTenantId();
        if (tenantId == Guid.Empty) return Error.Unauthorized("Tenant context is not resolved.");
        var branchId = _branch.GetBranchId();

        // Aggregate counts across Vehicles, Routes, Assignments, and expiring docs
        const string sql = @"
SELECT
  (SELECT COUNT(*) FROM Vehicles v INNER JOIN Branches b ON b.Id = v.BranchId
     WHERE b.TenantId = @t AND (@b IS NULL OR v.BranchId = @b) AND v.IsDeleted = 0) AS VehicleTotal,
  (SELECT COUNT(*) FROM Vehicles v INNER JOIN Branches b ON b.Id = v.BranchId
     WHERE b.TenantId = @t AND (@b IS NULL OR v.BranchId = @b) AND v.IsDeleted = 0 AND v.Status = 'active') AS VehicleActive,
  (SELECT COUNT(*) FROM Vehicles v INNER JOIN Branches b ON b.Id = v.BranchId
     WHERE b.TenantId = @t AND (@b IS NULL OR v.BranchId = @b) AND v.IsDeleted = 0 AND v.Status = 'maintenance') AS VehicleMaintenance,
  (SELECT COUNT(*) FROM Vehicles v INNER JOIN Branches b ON b.Id = v.BranchId
     WHERE b.TenantId = @t AND (@b IS NULL OR v.BranchId = @b) AND v.IsDeleted = 0 AND v.Status = 'retired') AS VehicleRetired,
  (SELECT COUNT(*) FROM Routes r INNER JOIN Branches b ON b.Id = r.BranchId
     WHERE b.TenantId = @t AND (@b IS NULL OR r.BranchId = @b) AND r.IsDeleted = 0) AS RouteTotal,
  (SELECT COUNT(*) FROM Routes r INNER JOIN Branches b ON b.Id = r.BranchId
     WHERE b.TenantId = @t AND (@b IS NULL OR r.BranchId = @b) AND r.IsDeleted = 0 AND r.Status = 'active') AS RouteActive,
  (SELECT COUNT(*) FROM TransportAssignments a WHERE a.TenantId = @t AND a.IsCurrent = 1 AND a.IsDeleted = 0
     AND (@b IS NULL OR 1=1)) AS CurrentAssignments,
  (SELECT COUNT(*) FROM Vehicles v INNER JOIN Branches b ON b.Id = v.BranchId
     WHERE b.TenantId = @t AND (@b IS NULL OR v.BranchId = @b) AND v.IsDeleted = 0
       AND ((v.InsuranceValidUntil IS NOT NULL AND v.InsuranceValidUntil <= DATEADD(DAY, 30, CAST(GETUTCDATE() AS DATE)))
         OR (v.PucValidUntil IS NOT NULL AND v.PucValidUntil <= DATEADD(DAY, 30, CAST(GETUTCDATE() AS DATE)))
         OR (v.FitnessValidUntil IS NOT NULL AND v.FitnessValidUntil <= DATEADD(DAY, 30, CAST(GETUTCDATE() AS DATE))))) AS ExpiringDocumentsCount,
  (SELECT COUNT(*) FROM Vehicles v INNER JOIN Branches b ON b.Id = v.BranchId
     WHERE b.TenantId = @t AND (@b IS NULL OR v.BranchId = @b) AND v.IsDeleted = 0
       AND ((v.InsuranceValidUntil IS NOT NULL AND v.InsuranceValidUntil < CAST(GETUTCDATE() AS DATE))
         OR (v.PucValidUntil IS NOT NULL AND v.PucValidUntil < CAST(GETUTCDATE() AS DATE))
         OR (v.FitnessValidUntil IS NOT NULL AND v.FitnessValidUntil < CAST(GETUTCDATE() AS DATE)))) AS ExpiredDocumentsCount;";

        var result = (await _repo.ExecuteRawSqlAsync(sql, new { t = tenantId, b = branchId }, ct)).FirstOrDefault();
        return Result.Success(result ?? new TransportDashboardResponse());
    }
}
