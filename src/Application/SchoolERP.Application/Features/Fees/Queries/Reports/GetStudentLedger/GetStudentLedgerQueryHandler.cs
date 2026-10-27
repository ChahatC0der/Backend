using MediatR;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Fees.DTOs.Reports;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Fees.Queries.Reports.GetStudentLedger;

public sealed class GetStudentLedgerQueryHandler : IRequestHandler<GetStudentLedgerQuery, Result<IReadOnlyList<StudentLedgerRow>>>
{
    private readonly IDapperRepository<StudentLedgerRow> _repo;
    private readonly ICurrentTenantService _tenant;

    public GetStudentLedgerQueryHandler(
        IDapperRepository<StudentLedgerRow> repo,
        ICurrentTenantService tenant)
    { _repo = repo; _tenant = tenant; }

    public async Task<Result<IReadOnlyList<StudentLedgerRow>>> Handle(GetStudentLedgerQuery request, CancellationToken ct)
    {
        var tenantId = _tenant.GetTenantId();
        if (tenantId == Guid.Empty) return Error.Unauthorized("Tenant context is not resolved.");

        const string sql = @"
SELECT i.CreatedAt AS EntryDate, 'FeeBill' AS EntryType, i.InvoiceNumber AS Reference,
       i.NetPayable AS Debit, CAST(0 AS DECIMAL(18,2)) AS Credit, i.Status
FROM Invoices i
WHERE i.StudentId=@sid AND i.TenantId=@t AND i.DeletedAt IS NULL AND (@ay IS NULL OR i.AcademicYearId=@ay)
UNION ALL
SELECT p.PaymentDate AS EntryDate, 'FeeReceipt' AS EntryType, p.ReceiptNumber AS Reference,
       CAST(0 AS DECIMAL(18,2)) AS Debit, p.Amount AS Credit, p.Status
FROM Payments p
WHERE p.StudentId=@sid AND p.TenantId=@t AND p.DeletedAt IS NULL
UNION ALL
SELECT cl.CreatedAt AS EntryDate, 'Wallet' AS EntryType, CAST(cl.Id AS NVARCHAR(50)) AS Reference,
       CASE WHEN cl.Amount < 0 THEN -cl.Amount ELSE 0 END AS Debit,
       CASE WHEN cl.Amount > 0 THEN cl.Amount ELSE 0 END AS Credit,
       cl.TransactionType AS Status
FROM StudentCreditLedger cl
WHERE cl.StudentId=@sid AND cl.TenantId=@t
UNION ALL
SELECT r.CreatedAt AS EntryDate, 'Refund' AS EntryType, CAST(r.Id AS NVARCHAR(50)) AS Reference,
       CAST(0 AS DECIMAL(18,2)) AS Debit, r.Amount AS Credit, r.Status
FROM Refunds r
WHERE r.StudentId=@sid AND r.TenantId=@t
ORDER BY EntryDate DESC;";

        var rows = (await _repo.ExecuteRawSqlAsync(sql,
            new { sid = request.StudentId, t = tenantId, ay = request.AcademicYearId }, ct)).ToList();

        // Running balance oldest  newest; keep original (newest-first) order
        var ordered = rows.OrderBy(r => r.EntryDate).ToList();
        decimal balance = 0m;
        foreach (var r in ordered) { balance += r.Credit - r.Debit; r.RunningBalance = balance; }

        return Result.Success<IReadOnlyList<StudentLedgerRow>>(rows);
    }
}
