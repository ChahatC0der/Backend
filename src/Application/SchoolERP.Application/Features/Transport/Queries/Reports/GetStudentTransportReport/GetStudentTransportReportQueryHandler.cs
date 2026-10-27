using MediatR;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Transport.DTOs.Reports;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Transport.Queries.Reports.GetStudentTransportReport;

/// <summary>Flat list of current transport assignments with parent mobile, route, stop, times, vehicle.</summary>
public sealed class GetStudentTransportReportQueryHandler : IRequestHandler<GetStudentTransportReportQuery, Result<IReadOnlyList<StudentTransportReportRow>>>
{
    private readonly IDapperRepository<StudentTransportReportRow> _repo;
    private readonly ICurrentTenantService _tenant;
    private readonly ICurrentBranchService _branch;

    public GetStudentTransportReportQueryHandler(
        IDapperRepository<StudentTransportReportRow> repo,
        ICurrentTenantService tenant, ICurrentBranchService branch)
    { _repo = repo; _tenant = tenant; _branch = branch; }

    public async Task<Result<IReadOnlyList<StudentTransportReportRow>>> Handle(GetStudentTransportReportQuery request, CancellationToken ct)
    {
        var tenantId = _tenant.GetTenantId();
        if (tenantId == Guid.Empty) return Error.Unauthorized("Tenant context is not resolved.");
        var branchId = _branch.GetBranchId();

        const string sql = @"
SELECT s.Id AS StudentId, s.EnrollmentId,
       LTRIM(RTRIM(s.FirstName + ' ' + ISNULL(s.LastName, ''))) AS StudentName,
       (SELECT TOP 1 p.Mobile FROM Parents p WHERE p.StudentId = s.Id AND p.IsPrimary = 1 AND p.DeletedAt IS NULL) AS ParentMobile,
       pr.Name AS PickupRouteName, ps.Name AS PickupStopName, ps.PickupTime, pv.VehicleNumber AS PickupVehicleNumber,
       dr.Name AS DropRouteName, ds.Name AS DropStopName, ds.DropTime, dv.VehicleNumber AS DropVehicleNumber
FROM TransportAssignments a
INNER JOIN Students s ON s.Id = a.StudentId AND s.DeletedAt IS NULL
INNER JOIN Branches br ON br.Id = s.BranchId
LEFT JOIN Routes pr ON pr.Id = a.PickupRouteId
LEFT JOIN Stops ps ON ps.Id = a.PickupStopId
LEFT JOIN Vehicles pv ON pv.Id = a.PickupVehicleId
LEFT JOIN Routes dr ON dr.Id = a.DropRouteId
LEFT JOIN Stops ds ON ds.Id = a.DropStopId
LEFT JOIN Vehicles dv ON dv.Id = a.DropVehicleId
WHERE a.TenantId = @t AND (@b IS NULL OR br.Id = @b) AND a.IsCurrent = 1 AND a.IsDeleted = 0
  AND (@ay IS NULL OR a.AcademicYearId = @ay)
ORDER BY s.FirstName, s.LastName;";

        var rows = await _repo.ExecuteRawSqlAsync(sql,
            new { t = tenantId, b = branchId, ay = request.AcademicYearId }, ct);
        return Result.Success<IReadOnlyList<StudentTransportReportRow>>(rows.ToList());
    }
}
