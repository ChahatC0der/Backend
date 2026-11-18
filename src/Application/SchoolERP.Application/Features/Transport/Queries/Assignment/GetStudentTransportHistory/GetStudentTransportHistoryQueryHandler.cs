
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Transport.DTOs.Assignment;
using SchoolERP.Domain.Shared.Results;
using TransportAssignmentEntity = SchoolERP.Domain.Transport.Entities.TransportAssignment;

namespace SchoolERP.Application.Features.Transport.Queries.Assignment.GetStudentTransportHistory;

public sealed class GetStudentTransportHistoryQueryHandler
    : IRequestHandler<GetStudentTransportHistoryQuery, Result<IReadOnlyList<TransportAssignmentResponse>>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;

    public GetStudentTransportHistoryQueryHandler(
        IApplicationDbContext db,
        ICurrentTenantService tenant)
    {
        _db = db;
        _tenant = tenant;
    }

    public async Task<Result<IReadOnlyList<TransportAssignmentResponse>>> Handle(
        GetStudentTransportHistoryQuery request,
        CancellationToken ct)
    {
        var tenantId = _tenant.GetTenantId();

        if (tenantId == Guid.Empty)
            return Error.Unauthorized("Tenant context is not resolved.");

        if (request.StudentId <= 0)
            return Error.Validation("Student ID must be greater than zero.");

        var rows = await _db.Set<TransportAssignmentEntity>()
            .AsNoTracking()
            .Where(a =>
                a.TenantId == tenantId &&
                a.StudentId == request.StudentId &&
                !a.IsDeleted)
            .Include(a => a.Student)
            .Include(a => a.PickupVehicle)
            .Include(a => a.PickupRoute)
            .Include(a => a.PickupStop)
            .Include(a => a.DropVehicle)
            .Include(a => a.DropRoute)
            .Include(a => a.DropStop)
            .OrderByDescending(a => a.ValidFrom)
            .ThenByDescending(a => a.Id)
            .Take(100)
            .ToListAsync(ct);

        return Result.Success<IReadOnlyList<TransportAssignmentResponse>>(
            rows.Select(TransportAssignmentResponse.FromEntity).ToList());
    }
}
