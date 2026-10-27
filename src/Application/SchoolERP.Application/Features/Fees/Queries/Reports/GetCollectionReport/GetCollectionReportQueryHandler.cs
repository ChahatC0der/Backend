using MediatR;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Fees.DTOs.Reports;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Fees.Queries.Reports.GetCollectionReport;

public sealed class GetCollectionReportQueryHandler : IRequestHandler<GetCollectionReportQuery, Result<IReadOnlyList<CollectionReportRow>>>
{
    private readonly IDapperRepository<CollectionReportRow> _repo;
    private readonly ICurrentTenantService _tenant;
    private readonly ICurrentBranchService _branch;

    public GetCollectionReportQueryHandler(
        IDapperRepository<CollectionReportRow> repo,
        ICurrentTenantService tenant, ICurrentBranchService branch)
    { _repo = repo; _tenant = tenant; _branch = branch; }

    public async Task<Result<IReadOnlyList<CollectionReportRow>>> Handle(GetCollectionReportQuery request, CancellationToken ct)
    {
        var tenantId = _tenant.GetTenantId();
        if (tenantId == Guid.Empty) return Error.Unauthorized("Tenant context is not resolved.");
        var branchId = _branch.GetBranchId();
        var from = request.From ?? DateTime.UtcNow.AddDays(-30);
        var to = request.To ?? DateTime.UtcNow;

        const string sql = @"
SELECT Mode, CollectedBy, COUNT(*) AS PaymentCount, SUM(Amount) AS TotalCollected
FROM Payments
WHERE TenantId=@t AND (@b IS NULL OR BranchId=@b) AND DeletedAt IS NULL
  AND Status='completed' AND PaymentDate BETWEEN @from AND @to
GROUP BY Mode, CollectedBy ORDER BY TotalCollected DESC;";

        var rows = await _repo.ExecuteRawSqlAsync(sql,
            new { t = tenantId, b = branchId, from, to }, ct);
        return Result.Success<IReadOnlyList<CollectionReportRow>>(rows.ToList());
    }
}
