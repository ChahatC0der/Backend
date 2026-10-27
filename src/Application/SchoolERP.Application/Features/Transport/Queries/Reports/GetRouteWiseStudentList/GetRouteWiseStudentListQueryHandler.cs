using MediatR;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Transport.DTOs.Reports;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Transport.Queries.Reports.GetRouteWiseStudentList;

/// <summary>Grouped by route → stop. Ordered for emergency use (route, stop, student).</summary>
public sealed class GetRouteWiseStudentListQueryHandler : IRequestHandler<GetRouteWiseStudentListQuery, Result<IReadOnlyList<RouteWiseStudentRow>>>
{
    private readonly IDapperRepository<RouteWiseStudentRow> _repo;
    private readonly ICurrentTenantService _tenant;
    private readonly ICurrentBranchService _branch;

    public GetRouteWiseStudentListQueryHandler(
        IDapperRepository<RouteWiseStudentRow> repo,
        ICurrentTenantService tenant, ICurrentBranchService branch)
    { _repo = repo; _tenant = tenant; _branch = branch; }

    public async Task<Result<IReadOnlyList<RouteWiseStudentRow>>> Handle(GetRouteWiseStudentListQuery request, CancellationToken ct)
    {
        var tenantId = _tenant.GetTenantId();
        if (tenantId == Guid.Empty) return Error.Unauthorized("Tenant context is not resolved.");
        var branchId = _branch.GetBranchId();

        const string sql = @"
SELECT r.Id AS RouteId, r.Name AS RouteName, r.Code AS RouteCode,
       st.Id AS StopId, st.Name AS StopName, st.StopOrder, st.PickupTime,
       v.VehicleNumber,
       s.Id AS StudentId,
       LTRIM(RTRIM(s.FirstName + ' ' + ISNULL(s.LastName, ''))) AS StudentName,
       s.EnrollmentId,
       (SELECT TOP 1 p.Mobile FROM Parents p WHERE p.StudentId = s.Id AND p.IsPrimary = 1 AND p.DeletedAt IS NULL) AS ParentMobile
FROM TransportAssignments a
INNER JOIN Students s ON s.Id = a.StudentId AND s.DeletedAt IS NULL
INNER JOIN Branches br ON br.Id = s.BranchId
INNER JOIN Routes r ON r.Id = a.PickupRouteId
INNER JOIN Stops st ON st.Id = a.PickupStopId
INNER JOIN Vehicles v ON v.Id = a.PickupVehicleId
WHERE a.TenantId = @t AND (@b IS NULL OR br.Id = @b) AND a.IsCurrent = 1 AND a.IsDeleted = 0
  AND (@rid IS NULL OR r.Id = @rid)
  AND (@ay IS NULL OR a.AcademicYearId = @ay)
ORDER BY r.Name, st.StopOrder, s.FirstName;";

        var rows = await _repo.ExecuteRawSqlAsync(sql,
            new { t = tenantId, b = branchId, rid = request.RouteId, ay = request.AcademicYearId }, ct);
        return Result.Success<IReadOnlyList<RouteWiseStudentRow>>(rows.ToList());
    }
}
