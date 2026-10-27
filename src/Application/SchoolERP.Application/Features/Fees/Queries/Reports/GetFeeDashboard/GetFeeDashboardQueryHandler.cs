using MediatR;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Fees.DTOs.Reports;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Fees.Queries.Reports.GetFeeDashboard;

public sealed class GetFeeDashboardQueryHandler : IRequestHandler<GetFeeDashboardQuery, Result<FeeDashboardResponse>>
{
    private readonly IDapperRepository<FeeDashboardResponse> _kpiRepo;
    private readonly IDapperRepository<DashboardBreakdown> _breakdownRepo;
    private readonly ICurrentTenantService _tenant;
    private readonly ICurrentBranchService _branch;

    public GetFeeDashboardQueryHandler(
        IDapperRepository<FeeDashboardResponse> kpiRepo,
        IDapperRepository<DashboardBreakdown> breakdownRepo,
        ICurrentTenantService tenant, ICurrentBranchService branch)
    { _kpiRepo = kpiRepo; _breakdownRepo = breakdownRepo; _tenant = tenant; _branch = branch; }

    public async Task<Result<FeeDashboardResponse>> Handle(GetFeeDashboardQuery request, CancellationToken ct)
    {
        var tenantId = _tenant.GetTenantId();
        if (tenantId == Guid.Empty) return Error.Unauthorized("Tenant context is not resolved.");
        var branchId = _branch.GetBranchId();

        var p = new { t = tenantId, b = branchId };

        const string kpiSql = @"
SELECT
  ISNULL((SELECT SUM(Amount) FROM Payments WHERE TenantId=@t AND (@b IS NULL OR BranchId=@b) AND DeletedAt IS NULL AND Status='completed' AND CAST(PaymentDate AS DATE) = CAST(GETUTCDATE() AS DATE)),0) AS CollectionToday,
  ISNULL((SELECT SUM(Amount) FROM Payments WHERE TenantId=@t AND (@b IS NULL OR BranchId=@b) AND DeletedAt IS NULL AND Status='completed' AND PaymentDate >= DATEADD(DAY,-7,GETUTCDATE())),0) AS CollectionWeek,
  ISNULL((SELECT SUM(Amount) FROM Payments WHERE TenantId=@t AND (@b IS NULL OR BranchId=@b) AND DeletedAt IS NULL AND Status='completed' AND PaymentDate >= DATEADD(DAY,-30,GETUTCDATE())),0) AS CollectionMonth,
  ISNULL((SELECT SUM(Amount) FROM Payments WHERE TenantId=@t AND (@b IS NULL OR BranchId=@b) AND DeletedAt IS NULL AND Status='completed'),0) AS CollectionYear,
  ISNULL((SELECT SUM(NetPayable - PaidAmount - CreditAppliedAmount) FROM Invoices WHERE TenantId=@t AND (@b IS NULL OR BranchId=@b) AND DeletedAt IS NULL AND Status NOT IN ('cancelled','paid') AND (NetPayable - PaidAmount - CreditAppliedAmount) > 0),0) AS TotalOutstanding,
  ISNULL((SELECT SUM(NetPayable - PaidAmount - CreditAppliedAmount) FROM Invoices WHERE TenantId=@t AND (@b IS NULL OR BranchId=@b) AND DeletedAt IS NULL AND Status NOT IN ('cancelled','paid') AND DueDate < GETUTCDATE() AND (NetPayable - PaidAmount - CreditAppliedAmount) > 0),0) AS TotalOverdue,
  ISNULL((SELECT COUNT(*) FROM Invoices WHERE TenantId=@t AND (@b IS NULL OR BranchId=@b) AND DeletedAt IS NULL AND Status NOT IN ('cancelled','paid') AND DueDate < GETUTCDATE() AND (NetPayable - PaidAmount - CreditAppliedAmount) > 0),0) AS OverdueInvoiceCount;";

        const string byHeadSql = @"
SELECT fh.Name AS Label, ISNULL(SUM(il.Amount - il.ConcessionApplied),0) AS Amount
FROM InvoiceLines il INNER JOIN Invoices i ON i.Id = il.InvoiceId
INNER JOIN FeeHeads fh ON fh.Id = il.FeeHeadId
WHERE i.TenantId=@t AND (@b IS NULL OR i.BranchId=@b) AND i.DeletedAt IS NULL AND il.IsLateFeeLine = 0
GROUP BY fh.Name ORDER BY Amount DESC;";

        const string byClassSql = @"
SELECT CAST(i.ClassId AS NVARCHAR(50)) AS Label, ISNULL(SUM(i.NetPayable - i.PaidAmount - i.CreditAppliedAmount),0) AS Amount
FROM Invoices i
WHERE i.TenantId=@t AND (@b IS NULL OR i.BranchId=@b) AND i.DeletedAt IS NULL AND i.Status NOT IN ('cancelled','paid')
GROUP BY i.ClassId ORDER BY Amount DESC;";

        var kpi = (await _kpiRepo.ExecuteRawSqlAsync(kpiSql, p, ct)).FirstOrDefault();
        if (kpi is null) return Result.Success(new FeeDashboardResponse());

        kpi.ByFeeHead = (await _breakdownRepo.ExecuteRawSqlAsync(byHeadSql, p, ct)).ToList();
        kpi.ByClass = (await _breakdownRepo.ExecuteRawSqlAsync(byClassSql, p, ct)).ToList();

        return Result.Success(kpi);
    }
}
