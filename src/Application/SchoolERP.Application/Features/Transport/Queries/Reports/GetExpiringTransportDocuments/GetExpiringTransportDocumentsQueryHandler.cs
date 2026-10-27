using MediatR;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Transport.DTOs.Reports;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Transport.Queries.Reports.GetExpiringTransportDocuments;

/// <summary>
/// Manual endpoint — no background job sends notifications in Phase 1.
/// Admin queries this endpoint on-demand, frontend uses response to alert Transport Manager.
/// Returns vehicle insurance/PUC/fitness expiring within DaysAhead or already expired.
/// </summary>
public sealed class GetExpiringTransportDocumentsQueryHandler : IRequestHandler<GetExpiringTransportDocumentsQuery, Result<IReadOnlyList<ExpiringDocumentAlertRow>>>
{
    private readonly IDapperRepository<ExpiringDocumentAlertRow> _repo;
    private readonly ICurrentTenantService _tenant;
    private readonly ICurrentBranchService _branch;

    public GetExpiringTransportDocumentsQueryHandler(
        IDapperRepository<ExpiringDocumentAlertRow> repo,
        ICurrentTenantService tenant, ICurrentBranchService branch)
    { _repo = repo; _tenant = tenant; _branch = branch; }

    public async Task<Result<IReadOnlyList<ExpiringDocumentAlertRow>>> Handle(GetExpiringTransportDocumentsQuery request, CancellationToken ct)
    {
        var tenantId = _tenant.GetTenantId();
        if (tenantId == Guid.Empty) return Error.Unauthorized("Tenant context is not resolved.");
        var branchId = _branch.GetBranchId();
        var days = request.DaysAhead <= 0 || request.DaysAhead > 365 ? 30 : request.DaysAhead;

        // UNION across insurance / puc / fitness with days-until-expiry computed
        const string sql = @"
SELECT v.Id AS VehicleId, v.VehicleNumber, 'insurance' AS DocumentType,
       v.InsuranceValidUntil AS ExpiryDate,
       DATEDIFF(DAY, CAST(GETUTCDATE() AS DATE), v.InsuranceValidUntil) AS DaysUntilExpiry,
       CASE WHEN v.InsuranceValidUntil < CAST(GETUTCDATE() AS DATE) THEN 1 ELSE 0 END AS IsExpired
FROM Vehicles v
INNER JOIN Branches b ON b.Id = v.BranchId
WHERE b.TenantId = @t AND (@b IS NULL OR v.BranchId = @b) AND v.IsDeleted = 0
  AND v.InsuranceValidUntil IS NOT NULL
  AND v.InsuranceValidUntil <= DATEADD(DAY, @days, CAST(GETUTCDATE() AS DATE))

UNION ALL

SELECT v.Id, v.VehicleNumber, 'puc', v.PucValidUntil,
       DATEDIFF(DAY, CAST(GETUTCDATE() AS DATE), v.PucValidUntil),
       CASE WHEN v.PucValidUntil < CAST(GETUTCDATE() AS DATE) THEN 1 ELSE 0 END
FROM Vehicles v
INNER JOIN Branches b ON b.Id = v.BranchId
WHERE b.TenantId = @t AND (@b IS NULL OR v.BranchId = @b) AND v.IsDeleted = 0
  AND v.PucValidUntil IS NOT NULL
  AND v.PucValidUntil <= DATEADD(DAY, @days, CAST(GETUTCDATE() AS DATE))

UNION ALL

SELECT v.Id, v.VehicleNumber, 'fitness', v.FitnessValidUntil,
       DATEDIFF(DAY, CAST(GETUTCDATE() AS DATE), v.FitnessValidUntil),
       CASE WHEN v.FitnessValidUntil < CAST(GETUTCDATE() AS DATE) THEN 1 ELSE 0 END
FROM Vehicles v
INNER JOIN Branches b ON b.Id = v.BranchId
WHERE b.TenantId = @t AND (@b IS NULL OR v.BranchId = @b) AND v.IsDeleted = 0
  AND v.FitnessValidUntil IS NOT NULL
  AND v.FitnessValidUntil <= DATEADD(DAY, @days, CAST(GETUTCDATE() AS DATE))

ORDER BY IsExpired DESC, DaysUntilExpiry ASC, VehicleNumber, DocumentType;";

        var rows = await _repo.ExecuteRawSqlAsync(sql,
            new { t = tenantId, b = branchId, days }, ct);
        return Result.Success<IReadOnlyList<ExpiringDocumentAlertRow>>(rows.ToList());
    }
}
