using System.Globalization;
using System.Reflection;
using System.Text;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SchoolERP.API.Authorization;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Fees.Queries.Reports.GetCollectionReport;
using SchoolERP.Application.Features.Fees.Queries.Reports.GetConcessionReport;
using SchoolERP.Application.Features.Fees.Queries.Reports.GetCreditBalanceReport;
using SchoolERP.Application.Features.Fees.Queries.Reports.GetFeeDashboard;
using SchoolERP.Application.Features.Fees.Queries.Reports.GetFinancialAuditLogs;
using SchoolERP.Application.Features.Fees.Queries.Reports.GetOutstandingReport;
using SchoolERP.Application.Features.Fees.Queries.Reports.GetOverdueDefaultersReport;
using SchoolERP.Application.Features.Fees.Queries.Reports.GetStudentLedger;
using SchoolERP.Domain.Shared.Permissions;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.API.Controllers.Fees;

[ApiController]
[Route("api/fees/reports")]
public sealed class FeeReportsController : BaseApiController
{
    private const string ExcelMime = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
    private readonly IMediator _mediator;
    private readonly IExcelExportService _excel;

    public FeeReportsController(IMediator mediator, IExcelExportService excel)
    { _mediator = mediator; _excel = excel; }

    [HttpGet("dashboard")]
    [HasPermission(PermissionNames.FeeReportRead)]
    public async Task<IActionResult> Dashboard([FromQuery] GetFeeDashboardQuery query, CancellationToken ct)
        => HandleResult(await _mediator.Send(query, ct));

    [HttpGet("outstanding")]
    [HasPermission(PermissionNames.FeeReportRead)]
    public async Task<IActionResult> Outstanding([FromQuery] GetOutstandingReportQuery query, [FromQuery] string? format, CancellationToken ct)
    {
        var r = await _mediator.Send(query, ct);
        if (r.IsFailure) return HandleResult(r);
        return FormatReport(r.Value, "Outstanding", format);
    }

    [HttpGet("defaulters")]
    [HasPermission(PermissionNames.FeeReportRead)]
    public async Task<IActionResult> Defaulters([FromQuery] GetOverdueDefaultersReportQuery query, [FromQuery] string? format, CancellationToken ct)
    {
        var r = await _mediator.Send(query, ct);
        if (r.IsFailure) return HandleResult(r);
        return FormatReport(r.Value, "Defaulters", format);
    }

    [HttpGet("collection")]
    [HasPermission(PermissionNames.FeeReportRead)]
    public async Task<IActionResult> Collection([FromQuery] GetCollectionReportQuery query, [FromQuery] string? format, CancellationToken ct)
    {
        var r = await _mediator.Send(query, ct);
        if (r.IsFailure) return HandleResult(r);
        return FormatReport(r.Value, "Collection", format);
    }

    [HttpGet("concessions")]
    [HasPermission(PermissionNames.FeeReportRead)]
    public async Task<IActionResult> Concessions([FromQuery] GetConcessionReportQuery query, [FromQuery] string? format, CancellationToken ct)
    {
        var r = await _mediator.Send(query, ct);
        if (r.IsFailure) return HandleResult(r);
        return FormatReport(r.Value, "Concessions", format);
    }

    [HttpGet("credit-balances")]
    [HasPermission(PermissionNames.FeeReportRead)]
    public async Task<IActionResult> CreditBalances([FromQuery] string? format, CancellationToken ct)
    {
        var r = await _mediator.Send(new GetCreditBalanceReportQuery(), ct);
        if (r.IsFailure) return HandleResult(r);
        return FormatReport(r.Value, "CreditBalances", format);
    }

    [HttpGet("student-ledger/{studentId:long}")]
    [HasPermission(PermissionNames.FeeReportRead)]
    public async Task<IActionResult> StudentLedger([FromRoute] long studentId, [FromQuery] long? academicYearId, [FromQuery] string? format, CancellationToken ct)
    {
        var r = await _mediator.Send(new GetStudentLedgerQuery(studentId, academicYearId), ct);
        if (r.IsFailure) return HandleResult(r);
        return FormatReport(r.Value, $"Ledger-{studentId}", format);
    }

    [HttpGet("audit-logs")]
    [HasPermission(PermissionNames.FeeReportAuditLogs)]
    public async Task<IActionResult> AuditLogs([FromQuery] GetFinancialAuditLogsQuery query, [FromQuery] string? format, CancellationToken ct)
    {
        var r = await _mediator.Send(query, ct);
        if (r.IsFailure) return HandleResult(r);
        return FormatReport(r.Value, "AuditLogs", format);
    }

    // FormatReport takes IReadOnlyList<T> directly (no Result<> wrapper)  no generic inference issue.
    private IActionResult FormatReport<T>(IReadOnlyList<T> rows, string sheetName, string? format) where T : class
    {
        if (string.Equals(format, "xlsx", StringComparison.OrdinalIgnoreCase))
        {
            var bytes = _excel.Export(rows, sheetName);
            return File(bytes, ExcelMime, $"{sheetName}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx");
        }

        if (string.Equals(format, "csv", StringComparison.OrdinalIgnoreCase))
        {
            var bytes = ToCsv(rows);
            return File(bytes, "text/csv", $"{sheetName}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv");
        }

        return Ok(rows);
    }

    private static byte[] ToCsv<T>(IReadOnlyList<T> rows) where T : class
    {
        if (rows.Count == 0) return Encoding.UTF8.GetBytes(string.Empty);
        var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var sb = new StringBuilder();
        sb.AppendLine(string.Join(",", props.Select(p => Escape(p.Name))));
        foreach (var row in rows)
        {
            sb.AppendLine(string.Join(",", props.Select(p =>
            {
                var v = p.GetValue(row);
                if (v is null) return "";
                if (v is DateTime dt) return dt.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                if (v is decimal d) return d.ToString("F2", CultureInfo.InvariantCulture);
                return Escape(v.ToString() ?? "");
            })));
        }
        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    private static string Escape(string s)
    {
        if (string.IsNullOrEmpty(s)) return "";
        if (s.Contains(",") || s.Contains("\"") || s.Contains("\n") || s.Contains("\r"))
            return "\"" + s.Replace("\"", "\"\"") + "\"";
        return s;
    }
}
