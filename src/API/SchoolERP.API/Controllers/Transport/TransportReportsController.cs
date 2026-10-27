using System.Globalization;
using System.Reflection;
using System.Text;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Transport.Queries.Reports.GetDriverReport;
using SchoolERP.Application.Features.Transport.Queries.Reports.GetExpiringTransportDocuments;
using SchoolERP.Application.Features.Transport.Queries.Reports.GetRouteWiseStudentList;
using SchoolERP.Application.Features.Transport.Queries.Reports.GetStudentTransportReport;
using SchoolERP.Application.Features.Transport.Queries.Reports.GetTransportDashboard;
using SchoolERP.Application.Features.Transport.Queries.Reports.GetVehicleMaintenanceReport;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.API.Controllers.Transport;

/// <summary>
/// TEMPORARY: [AllowAnonymous]. When RBAC ready, replace with
/// [HasPermission(PermissionNames.TransportReportRead)].
/// </summary>
[ApiController]
[Route("api/transport/reports")]
[AllowAnonymous]
public sealed class TransportReportsController : BaseApiController
{
    private const string ExcelMime = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
    private readonly IMediator _mediator;
    private readonly IExcelExportService _excel;

    public TransportReportsController(IMediator mediator, IExcelExportService excel)
    { _mediator = mediator; _excel = excel; }

    [HttpGet("dashboard")]
    [AllowAnonymous]  // TODO: [HasPermission(PermissionNames.TransportDashboardRead)]
    public async Task<IActionResult> Dashboard(CancellationToken ct)
        => HandleResult(await _mediator.Send(new GetTransportDashboardQuery(), ct));

    [HttpGet("students")]
    [AllowAnonymous]  // TODO: [HasPermission(PermissionNames.TransportReportRead)]
    public async Task<IActionResult> Students([FromQuery] long? academicYearId, [FromQuery] string? format, CancellationToken ct)
    {
        var r = await _mediator.Send(new GetStudentTransportReportQuery(academicYearId), ct);
        if (r.IsFailure) return HandleResult(r);
        return FormatReport(r.Value, "StudentTransport", format);
    }

    [HttpGet("routes/{routeId:long}/students")]
    [AllowAnonymous]  // TODO: [HasPermission(PermissionNames.TransportReportRead)]
    public async Task<IActionResult> RouteStudents([FromRoute] long routeId, [FromQuery] long? academicYearId, [FromQuery] string? format, CancellationToken ct)
    {
        var r = await _mediator.Send(new GetRouteWiseStudentListQuery(routeId, academicYearId), ct);
        if (r.IsFailure) return HandleResult(r);
        return FormatReport(r.Value, $"Route-{routeId}", format);
    }

    [HttpGet("route-wise")]
    [AllowAnonymous]  // TODO: [HasPermission(PermissionNames.TransportReportRead)]
    public async Task<IActionResult> RouteWise([FromQuery] long? academicYearId, [FromQuery] string? format, CancellationToken ct)
    {
        var r = await _mediator.Send(new GetRouteWiseStudentListQuery(null, academicYearId), ct);
        if (r.IsFailure) return HandleResult(r);
        return FormatReport(r.Value, "RouteWiseStudents", format);
    }

    [HttpGet("vehicle-maintenance")]
    [AllowAnonymous]  // TODO: [HasPermission(PermissionNames.TransportReportRead)]
    public async Task<IActionResult> Maintenance([FromQuery] long? vehicleId, [FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] string? format, CancellationToken ct)
    {
        var r = await _mediator.Send(new GetVehicleMaintenanceReportQuery(vehicleId, from, to), ct);
        if (r.IsFailure) return HandleResult(r);
        return FormatReport(r.Value, "Maintenance", format);
    }

    [HttpGet("drivers")]
    [AllowAnonymous]  // TODO: [HasPermission(PermissionNames.TransportReportRead)]
    public async Task<IActionResult> Drivers([FromQuery] bool? onlyCurrent, [FromQuery] string? format, CancellationToken ct)
    {
        var r = await _mediator.Send(new GetDriverReportQuery(onlyCurrent), ct);
        if (r.IsFailure) return HandleResult(r);
        return FormatReport(r.Value, "Drivers", format);
    }

    [HttpGet("expiring-documents")]
    [AllowAnonymous]  // TODO: [HasPermission(PermissionNames.TransportReportRead)]
    public async Task<IActionResult> Expiring([FromQuery] string? format, CancellationToken ct,[FromQuery] int daysAhead = 30)
    {
        var r = await _mediator.Send(new GetExpiringTransportDocumentsQuery(daysAhead), ct);
        if (r.IsFailure) return HandleResult(r);
        return FormatReport(r.Value, "ExpiringDocuments", format);
    }

    // -------- shared formatter --------
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
                if (v is DateOnly d) return d.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                if (v is TimeOnly t) return t.ToString("HH:mm:ss", CultureInfo.InvariantCulture);
                if (v is decimal dc) return dc.ToString("F2", CultureInfo.InvariantCulture);
                if (v is bool b) return b ? "true" : "false";
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
