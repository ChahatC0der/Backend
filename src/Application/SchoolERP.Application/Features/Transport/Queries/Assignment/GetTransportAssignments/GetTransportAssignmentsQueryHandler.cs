using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.DTOs;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Transport.DTOs.Assignment;
using SchoolERP.Domain.Shared.Results;
using TransportAssignmentEntity = SchoolERP.Domain.Transport.Entities.TransportAssignment;

namespace SchoolERP.Application.Features.Transport.Queries.Assignment.GetTransportAssignments;

public sealed class GetTransportAssignmentsQueryHandler : IRequestHandler<GetTransportAssignmentsQuery, Result<PagedResponse<TransportAssignmentResponse>>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;

    public GetTransportAssignmentsQueryHandler(IApplicationDbContext db, ICurrentTenantService tenant)
    { _db = db; _tenant = tenant; }

    public async Task<Result<PagedResponse<TransportAssignmentResponse>>> Handle(GetTransportAssignmentsQuery request, CancellationToken ct)
    {
        var tenantId = _tenant.GetTenantId();
        if (tenantId == Guid.Empty) return Error.Unauthorized("Tenant context is not resolved.");

        var page = request.Page <= 0 ? 1 : request.Page;
        var size = request.Size <= 0 ? 10 : Math.Min(request.Size, 200);

        var query = _db.Set<TransportAssignmentEntity>().AsNoTracking()
            .Where(a => a.TenantId == tenantId && !a.IsDeleted);

        if (request.AcademicYearId.HasValue) query = query.Where(a => a.AcademicYearId == request.AcademicYearId.Value);
        if (request.PickupRouteId.HasValue) query = query.Where(a => a.PickupRouteId == request.PickupRouteId.Value);
        if (request.PickupStopId.HasValue) query = query.Where(a => a.PickupStopId == request.PickupStopId.Value);
        if (request.PickupVehicleId.HasValue) query = query.Where(a => a.PickupVehicleId == request.PickupVehicleId.Value);
        if (request.IsCurrent.HasValue) query = query.Where(a => a.IsCurrent == request.IsCurrent.Value);

        var total = await query.CountAsync(ct);
        var rows = await query.OrderByDescending(a => a.Id)
            .Skip((page - 1) * size).Take(size).ToListAsync(ct);

        return Result.Success(new PagedResponse<TransportAssignmentResponse>
        {
            Data = rows.Select(TransportAssignmentResponse.FromEntity).ToList(),
            Page = page, Size = size, TotalCount = total,
            //TotalPages = (int)Math.Ceiling(total / (double)size),
        });
    }
}
