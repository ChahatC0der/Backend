using MediatR;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Fees.DTOs.Reports;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Fees.Queries.Reports.GetCreditBalanceReport;

public sealed class GetCreditBalanceReportQueryHandler : IRequestHandler<GetCreditBalanceReportQuery, Result<IReadOnlyList<CreditBalanceReportRow>>>
{
    private readonly IDapperRepository<CreditBalanceReportRow> _repo;
    private readonly ICurrentTenantService _tenant;
    private readonly ICurrentBranchService _branch;

    public GetCreditBalanceReportQueryHandler(
        IDapperRepository<CreditBalanceReportRow> repo,
        ICurrentTenantService tenant, ICurrentBranchService branch)
    { _repo = repo; _tenant = tenant; _branch = branch; }

    public async Task<Result<IReadOnlyList<CreditBalanceReportRow>>> Handle(GetCreditBalanceReportQuery request, CancellationToken ct)
    {
        var tenantId = _tenant.GetTenantId();
        if (tenantId == Guid.Empty) return Error.Unauthorized("Tenant context is not resolved.");
        var branchId = _branch.GetBranchId();

        const string sql = @"
WITH latest AS (
    SELECT StudentId, BalanceAfter, CreatedAt,
           ROW_NUMBER() OVER (PARTITION BY StudentId ORDER BY Id DESC) AS rn
    FROM StudentCreditLedger WHERE TenantId=@t
)
SELECT s.Id AS StudentId,
       LTRIM(RTRIM(s.FirstName + ' ' + ISNULL(s.LastName,''))) AS StudentName,
       s.EnrollmentId, latest.BalanceAfter AS Balance, latest.CreatedAt AS LastUpdatedAt
FROM Students s INNER JOIN latest ON latest.StudentId = s.Id AND latest.rn = 1
WHERE s.TenantId=@t AND (@b IS NULL OR s.BranchId=@b) AND s.DeletedAt IS NULL
  AND latest.BalanceAfter > 0
ORDER BY latest.BalanceAfter DESC;";

        var rows = await _repo.ExecuteRawSqlAsync(sql, new { t = tenantId, b = branchId }, ct);
        return Result.Success<IReadOnlyList<CreditBalanceReportRow>>(rows.ToList());
    }
}
