using MediatR;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Fees.DTOs.Reports;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Fees.Queries.Reports.GetOutstandingReport;

public sealed class GetOutstandingReportQueryHandler : IRequestHandler<GetOutstandingReportQuery, Result<IReadOnlyList<OutstandingReportRow>>>
{
    private readonly IDapperRepository<OutstandingReportRow> _repo;
    private readonly ICurrentTenantService _tenant;
    private readonly ICurrentBranchService _branch;

    public GetOutstandingReportQueryHandler(
        IDapperRepository<OutstandingReportRow> repo,
        ICurrentTenantService tenant, ICurrentBranchService branch)
    { _repo = repo; _tenant = tenant; _branch = branch; }

    public async Task<Result<IReadOnlyList<OutstandingReportRow>>> Handle(GetOutstandingReportQuery request, CancellationToken ct)
    {
        var tenantId = _tenant.GetTenantId();
        if (tenantId == Guid.Empty) return Error.Unauthorized("Tenant context is not resolved.");
        var branchId = _branch.GetBranchId();

        const string sql = @"
SELECT i.Id AS InvoiceId, i.InvoiceNumber, i.StudentId, i.ClassId, i.SectionId,
       i.PeriodLabel, i.DueDate, i.NetPayable, i.PaidAmount, i.CreditAppliedAmount,
       (i.NetPayable - i.PaidAmount - i.CreditAppliedAmount) AS Outstanding, i.Status
FROM Invoices i
WHERE i.TenantId=@t AND (@b IS NULL OR i.BranchId=@b) AND i.DeletedAt IS NULL
  AND (@ay IS NULL OR i.AcademicYearId=@ay) AND (@cid IS NULL OR i.ClassId=@cid)
  AND i.Status NOT IN ('cancelled','paid')
  AND (i.NetPayable - i.PaidAmount - i.CreditAppliedAmount) > 0
ORDER BY i.DueDate;";

        var rows = await _repo.ExecuteRawSqlAsync(sql,
            new { t = tenantId, b = branchId, ay = request.AcademicYearId, cid = request.ClassId }, ct);
        return Result.Success<IReadOnlyList<OutstandingReportRow>>(rows.ToList());
    }
}
