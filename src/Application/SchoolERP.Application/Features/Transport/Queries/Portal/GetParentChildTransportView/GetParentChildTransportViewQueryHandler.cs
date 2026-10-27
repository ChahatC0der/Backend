
using MediatR;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Fees.Builders;
using SchoolERP.Application.Features.Transport.DTOs.Portal;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Transport.Queries.Portal.GetParentChildTransportView;

/// <summary>
/// Parent portal: own child's transport view only.
/// Auth via ParentChildResolver — parent A cannot see parent B's child.
/// Fee amount never exposed to parent.
/// </summary>
public sealed class GetParentChildTransportViewQueryHandler : IRequestHandler<GetParentChildTransportViewQuery, Result<ParentTransportResponse?>>
{
    private readonly IDapperRepository<ParentTransportResponse> _repo;
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;
    private readonly ICurrentUserService _user;
    private readonly ICurrentAcademicYearProvider _ay;

    public GetParentChildTransportViewQueryHandler(
        IDapperRepository<ParentTransportResponse> repo,
        IApplicationDbContext db,
        ICurrentTenantService tenant,
        ICurrentUserService user,
        ICurrentAcademicYearProvider ay)
    { _repo = repo; _db = db; _tenant = tenant; _user = user; _ay = ay; }

    public async Task<Result<ParentTransportResponse?>> Handle(GetParentChildTransportViewQuery request, CancellationToken ct)
    {
        var tenantId = _tenant.GetTenantId();
        if (tenantId == Guid.Empty) return Error.Unauthorized("Tenant context is not resolved.");

        // AUTH: current user must be parent of the student
        var currentUserId = _user.GetUserId() ?? 0;
        var auth = await ParentChildResolver.EnsureParentAsync(_db, currentUserId, request.StudentId, ct);
        if (auth.IsFailure) return auth.Error!;

        long ayId;
        if (request.AcademicYearId.HasValue) ayId = request.AcademicYearId.Value;
        else
        {
            var ay = await _ay.GetCurrentAsync(ct);
            if (ay is null) return Error.Validation("No current academic year configured.");
            ayId = ay.Id;
        }

        // Dapper single query — auth already validated, tenant scoping on WHERE
        const string sql = @"
SELECT s.Id AS StudentId,
       LTRIM(RTRIM(s.FirstName + ' ' + ISNULL(s.LastName, ''))) AS StudentName,
       s.EnrollmentId, a.AcademicYearId,
       pr.Name AS PickupRouteName, pr.Code AS PickupRouteCode,
       ps.Name AS PickupStopName, ps.Landmark AS PickupLandmark,
       CONVERT(varchar(5), ps.PickupTime) AS PickupTime,
       pv.VehicleNumber AS PickupVehicleNumber,
       dr.Name AS DropRouteName, ds.Name AS DropStopName,
       CONVERT(varchar(5), ds.DropTime) AS DropTime,
       dv.VehicleNumber AS DropVehicleNumber,
       a.ValidFrom, a.ValidTo
FROM TransportAssignments a
INNER JOIN Students s ON s.Id = a.StudentId
LEFT JOIN Routes pr ON pr.Id = a.PickupRouteId
LEFT JOIN Stops ps ON ps.Id = a.PickupStopId
LEFT JOIN Vehicles pv ON pv.Id = a.PickupVehicleId
LEFT JOIN Routes dr ON dr.Id = a.DropRouteId
LEFT JOIN Stops ds ON ds.Id = a.DropStopId
LEFT JOIN Vehicles dv ON dv.Id = a.DropVehicleId
WHERE a.TenantId = @t AND a.StudentId = @sid AND a.AcademicYearId = @ay
  AND a.IsCurrent = 1 AND a.IsDeleted = 0;";

        var row = (await _repo.ExecuteRawSqlAsync(sql,
            new { t = tenantId, sid = request.StudentId, ay = ayId }, ct)).FirstOrDefault();

        return Result.Success<ParentTransportResponse?>(row);
    }
}
