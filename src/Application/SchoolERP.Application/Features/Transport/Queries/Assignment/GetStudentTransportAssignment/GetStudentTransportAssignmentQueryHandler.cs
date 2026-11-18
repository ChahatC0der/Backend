
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Transport.DTOs.Assignment;
using SchoolERP.Domain.Shared.Results;
using TransportAssignmentEntity = SchoolERP.Domain.Transport.Entities.TransportAssignment;
using StudentEntity = SchoolERP.Domain.Student.Entities.Student;

namespace SchoolERP.Application.Features.Transport.Queries.Assignment.GetStudentTransportAssignment;

public sealed class GetStudentTransportAssignmentQueryHandler
    : IRequestHandler<GetStudentTransportAssignmentQuery, Result<TransportAssignmentResponse?>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;

    public GetStudentTransportAssignmentQueryHandler(
        IApplicationDbContext db,
        ICurrentTenantService tenant)
    {
        _db = db;
        _tenant = tenant;
    }

    public async Task<Result<TransportAssignmentResponse?>> Handle(
        GetStudentTransportAssignmentQuery request,
        CancellationToken ct)
    {
        var tenantId = _tenant.GetTenantId();

        if (tenantId == Guid.Empty)
            return Error.Unauthorized("Tenant context is not resolved.");

        if (request.StudentId <= 0)
            return Error.Validation("Student ID must be greater than zero.");

        if (request.AcademicYearId.HasValue &&
            request.AcademicYearId.Value <= 0)
            return Error.Validation("Academic Year ID must be greater than zero.");

        var studentExists = await _db.Set<StudentEntity>()
            .AsNoTracking()
            .AnyAsync(s =>
                s.Id == request.StudentId &&
                s.TenantId == tenantId &&
                !s.IsDeleted,
                ct);

        if (!studentExists)
            return Error.NotFound("Student", request.StudentId.ToString());

        var query = _db.Set<TransportAssignmentEntity>()
            .AsNoTracking()
            .Where(a =>
                a.TenantId == tenantId &&
                a.StudentId == request.StudentId &&
                a.IsCurrent &&
                !a.IsDeleted);

        if (request.AcademicYearId.HasValue)
        {
            query = query.Where(a =>
                a.AcademicYearId == request.AcademicYearId.Value);
        }

        var assignment = await query
            .Include(a => a.Student)
            .Include(a => a.PickupVehicle)
            .Include(a => a.PickupRoute)
            .Include(a => a.PickupStop)
            .Include(a => a.DropVehicle)
            .Include(a => a.DropRoute)
            .Include(a => a.DropStop)
            .OrderByDescending(a => a.ValidFrom)
            .ThenByDescending(a => a.Id)
            .FirstOrDefaultAsync(ct);

        if (assignment is null)
            return Result.Success<TransportAssignmentResponse?>(null);

        var response = TransportAssignmentResponse.FromEntity(assignment);

        return Result.Success<TransportAssignmentResponse?>(response);
    }
}
