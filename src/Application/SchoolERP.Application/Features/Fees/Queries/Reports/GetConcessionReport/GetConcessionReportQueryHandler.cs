using MediatR;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Fees.DTOs.Reports;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Fees.Queries.Reports.GetConcessionReport;

public sealed class GetConcessionReportQueryHandler : IRequestHandler<GetConcessionReportQuery, Result<IReadOnlyList<ConcessionReportRow>>>
{
    private readonly IDapperRepository<ConcessionReportRow> _repo;
    private readonly ICurrentTenantService _tenant;
    private readonly ICurrentBranchService _branch;

    public GetConcessionReportQueryHandler(
        IDapperRepository<ConcessionReportRow> repo,
        ICurrentTenantService tenant, ICurrentBranchService branch)
    { _repo = repo; _tenant = tenant; _branch = branch; }

    public async Task<Result<IReadOnlyList<ConcessionReportRow>>> Handle(GetConcessionReportQuery request, CancellationToken ct)
    {
        var tenantId = _tenant.GetTenantId();
        if (tenantId == Guid.Empty) return Error.Unauthorized("Tenant context is not resolved.");
        var branchId = _branch.GetBranchId();

        const string sql = @"
SELECT c.Id AS ConcessionId, c.Name AS ConcessionName, c.Type, c.Value,
       ISNULL(COUNT(DISTINCT ca.StudentId),0) AS ApprovedStudents,
       ISNULL(SUM(cu.AppliedAmount),0) AS TotalApplied
FROM Concessions c
LEFT JOIN ConcessionAllocations ca ON ca.ConcessionId = c.Id AND ca.Status='approved' AND (@ay IS NULL OR ca.AcademicYearId=@ay)
LEFT JOIN ConcessionUsages cu ON cu.ConcessionAllocationId = ca.Id
WHERE c.TenantId=@t AND (@b IS NULL OR c.BranchId=@b) AND c.DeletedAt IS NULL
GROUP BY c.Id, c.Name, c.Type, c.Value ORDER BY TotalApplied DESC;";

        var rows = await _repo.ExecuteRawSqlAsync(sql,
            new { t = tenantId, b = branchId, ay = request.AcademicYearId }, ct);
        return Result.Success<IReadOnlyList<ConcessionReportRow>>(rows.ToList());
    }
}
