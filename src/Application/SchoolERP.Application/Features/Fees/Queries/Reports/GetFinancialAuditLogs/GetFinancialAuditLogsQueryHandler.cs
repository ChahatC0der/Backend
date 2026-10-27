using MediatR;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Fees.DTOs.Reports;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Fees.Queries.Reports.GetFinancialAuditLogs;

public sealed class GetFinancialAuditLogsQueryHandler : IRequestHandler<GetFinancialAuditLogsQuery, Result<IReadOnlyList<AuditLogRow>>>
{
    private readonly IDapperRepository<AuditLogRow> _repo;
    private readonly ICurrentTenantService _tenant;

    public GetFinancialAuditLogsQueryHandler(
        IDapperRepository<AuditLogRow> repo,
        ICurrentTenantService tenant)
    { _repo = repo; _tenant = tenant; }

    public async Task<Result<IReadOnlyList<AuditLogRow>>> Handle(GetFinancialAuditLogsQuery request, CancellationToken ct)
    {
        var tenantId = _tenant.GetTenantId();
        if (tenantId == Guid.Empty) return Error.Unauthorized("Tenant context is not resolved.");
        var lim = request.Limit <= 0 || request.Limit > 500 ? 500 : request.Limit;

        const string sql = @"
SELECT TOP (@lim) Id, TenantId, TableName, RecordId, Action, OldValues, NewValues, PerformedBy, CreatedAt
FROM FinancialAuditLogs
WHERE TenantId=@t
  AND (@tn IS NULL OR TableName = @tn)
  AND (@from IS NULL OR CreatedAt >= @from)
  AND (@to IS NULL OR CreatedAt <= @to)
ORDER BY Id DESC;";

        var rows = await _repo.ExecuteRawSqlAsync(sql,
            new { t = tenantId, tn = request.TableName, from = request.From, to = request.To, lim }, ct);
        return Result.Success<IReadOnlyList<AuditLogRow>>(rows.ToList());
    }
}
