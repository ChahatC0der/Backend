using MediatR;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Fees.DTOs.Reports;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Fees.Queries.Reports.GetOverdueDefaultersReport;

public sealed class GetOverdueDefaultersReportQueryHandler : IRequestHandler<GetOverdueDefaultersReportQuery, Result<IReadOnlyList<DefaulterReportRow>>>
{
    private readonly IDapperRepository<DefaulterReportRow> _repo;
    private readonly ICurrentTenantService _tenant;
    private readonly ICurrentBranchService _branch;

    public GetOverdueDefaultersReportQueryHandler(
        IDapperRepository<DefaulterReportRow> repo,
        ICurrentTenantService tenant, ICurrentBranchService branch)
    { _repo = repo; _tenant = tenant; _branch = branch; }

    public async Task<Result<IReadOnlyList<DefaulterReportRow>>> Handle(GetOverdueDefaultersReportQuery request, CancellationToken ct)
    {
        var tenantId = _tenant.GetTenantId();
        if (tenantId == Guid.Empty) return Error.Unauthorized("Tenant context is not resolved.");
        var branchId = _branch.GetBranchId();
        var thr = request.OverdueThresholdDays <= 0 ? 30 : request.OverdueThresholdDays;

        const string sql = @"
SELECT i.Id AS InvoiceId, i.InvoiceNumber, i.StudentId,
       LTRIM(RTRIM(s.FirstName + ' ' + ISNULL(s.LastName,''))) AS StudentName,
       s.EnrollmentId, i.ClassId, i.SectionId, i.DueDate,
       DATEDIFF(DAY, i.DueDate, GETUTCDATE()) AS DaysOverdue,
       (i.NetPayable - i.PaidAmount - i.CreditAppliedAmount) AS Outstanding
FROM Invoices i INNER JOIN Students s ON s.Id = i.StudentId
WHERE i.TenantId=@t AND (@b IS NULL OR i.BranchId=@b) AND i.DeletedAt IS NULL
  AND i.Status NOT IN ('cancelled','paid')
  AND DATEDIFF(DAY, i.DueDate, GETUTCDATE()) >= @thr
  AND (i.NetPayable - i.PaidAmount - i.CreditAppliedAmount) > 0
ORDER BY DaysOverdue DESC;";

        var rows = await _repo.ExecuteRawSqlAsync(sql,
            new { t = tenantId, b = branchId, thr }, ct);
        return Result.Success<IReadOnlyList<DefaulterReportRow>>(rows.ToList());
    }
}
