using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Transport.DTOs.Assignment;
using SchoolERP.Domain.Shared.Results;
using RouteEntity = SchoolERP.Domain.Transport.Entities.Route;
using StopEntity = SchoolERP.Domain.Transport.Entities.Stop;
using VehicleEntity = SchoolERP.Domain.Transport.Entities.Vehicle;
using TransportAssignmentEntity = SchoolERP.Domain.Transport.Entities.TransportAssignment;

namespace SchoolERP.Application.Features.Transport.Commands.Assignment.CreateTransportAssignment;

public sealed class CreateTransportAssignmentCommandHandler : IRequestHandler<CreateTransportAssignmentCommand, Result<TransportAssignmentResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;

    public CreateTransportAssignmentCommandHandler(IApplicationDbContext db, ICurrentTenantService tenant)
    { _db = db; _tenant = tenant; }

    public async Task<Result<TransportAssignmentResponse>> Handle(CreateTransportAssignmentCommand request, CancellationToken ct)
    {
        var tenantId = _tenant.GetTenantId();
        if (tenantId == Guid.Empty) return Error.Unauthorized("Tenant context is not resolved.");

        var r = request.Request;

        // Validate structure (student, enrollment, vehicle, route, stop, stop-type)
        var validation = await AssignmentValidator.ValidateAsync(_db, tenantId, r.StudentId, r.AcademicYearId,
            r.PickupVehicleId, r.PickupRouteId, r.PickupStopId,
            r.DropVehicleId, r.DropRouteId, r.DropStopId, ct);
        if (validation.IsFailure) return validation.Error!;
        var scope = validation.Value;

        // Duplicate check: existing current assignment for (student, AY)
        var existing = await _db.Set<TransportAssignmentEntity>().AnyAsync(a =>
            a.TenantId == tenantId && a.StudentId == r.StudentId && a.AcademicYearId == r.AcademicYearId
            && a.IsCurrent && !a.IsDeleted, ct);
        if (existing)
            return Error.Conflict($"Student already has a current transport assignment for academic year {r.AcademicYearId}. Use ChangeTransportAssignment instead.");

        // Capacity check (warning, not block)
        var warnings = new List<string>();
        var vehicleIds = new List<long> { r.PickupVehicleId };
        if (r.DropVehicleId.HasValue && r.DropVehicleId.Value != r.PickupVehicleId)
            vehicleIds.Add(r.DropVehicleId.Value);

        var counts = await AssignmentValidator.GetVehicleCurrentCountsAsync(_db, tenantId, r.AcademicYearId, vehicleIds, ct);
        var pickupCount = counts.TryGetValue(r.PickupVehicleId, out var pc) ? pc : 0;
        if (pickupCount + 1 > scope.PickupVehicleCapacity)
            warnings.Add($"Pickup vehicle capacity exceeded: current {pickupCount}, capacity {scope.PickupVehicleCapacity}.");

        if (r.DropVehicleId.HasValue && r.DropVehicleId.Value != r.PickupVehicleId)
        {
            var dropCount = counts.TryGetValue(r.DropVehicleId.Value, out var dc) ? dc : 0;
            var dropVehicle = await _db.Set<VehicleEntity>().AsNoTracking()
                .Where(v => v.Id == r.DropVehicleId.Value).Select(v => v.Capacity).FirstOrDefaultAsync(ct);
            if (dropCount + 1 > dropVehicle)
                warnings.Add($"Drop vehicle capacity exceeded: current {dropCount}, capacity {dropVehicle}.");
        }

        if (warnings.Count > 0 && !r.OverrideCapacity)
            return Error.Conflict("Capacity exceeded. Set OverrideCapacity = true and provide OverrideReason to proceed. Warnings: " + string.Join(" | ", warnings));

        // Different pickup/drop route = warning
        if (r.DropRouteId.HasValue && r.DropRouteId.Value != r.PickupRouteId)
            warnings.Add("Pickup and drop routes differ — please notify the transport manager.");

        // --- Mutations ---
        var entity = new TransportAssignmentEntity
        {
            TenantId = tenantId,
            StudentId = r.StudentId,
            AcademicYearId = r.AcademicYearId,
            PickupVehicleId = r.PickupVehicleId,
            PickupRouteId = r.PickupRouteId,
            PickupStopId = r.PickupStopId,
            DropVehicleId = r.DropVehicleId,
            DropRouteId = r.DropRouteId,
            DropStopId = r.DropStopId,
            ValidFrom = r.ValidFrom,
            IsCurrent = true,
            CreatedAt = DateTime.UtcNow,
        };
        _db.Set<TransportAssignmentEntity>().Add(entity);
        await _db.SaveChangesAsync(ct);

        // TODO (audit): await _auditWriter.LogAsync("TransportAssignments", entity.Id, FinancialAuditAction.Create, null, entity, ct);

        // Enrich response
        var resp = await EnrichAsync(entity, ct);
        resp.Warnings = warnings;
        return Result.Success(resp);
    }

    private async Task<TransportAssignmentResponse> EnrichAsync(TransportAssignmentEntity e, CancellationToken ct)
    {
        var resp = TransportAssignmentResponse.FromEntity(e);
        resp.StudentName = await _db.Set<SchoolERP.Domain.Student.Entities.Student>().AsNoTracking()
            .Where(s => s.Id == e.StudentId)
            .Select(s => s.FirstName + " " + (s.LastName ?? ""))
            .FirstOrDefaultAsync(ct);
        resp.EnrollmentId = await _db.Set<SchoolERP.Domain.Student.Entities.Student>().AsNoTracking()
            .Where(s => s.Id == e.StudentId).Select(s => s.EnrollmentId).FirstOrDefaultAsync(ct);

        var routeIds = new[] { e.PickupRouteId, e.DropRouteId ?? 0 }.Where(x => x > 0).Distinct().ToList();
        var routes = await _db.Set<RouteEntity>().AsNoTracking()
            .Where(r => routeIds.Contains(r.Id))
            .ToDictionaryAsync(r => r.Id, r => new { r.Name, r.Code }, ct);

        if (routes.TryGetValue(e.PickupRouteId, out var pr)) { resp.PickupRouteName = pr.Name; resp.PickupRouteCode = pr.Code; }
        if (e.DropRouteId.HasValue && routes.TryGetValue(e.DropRouteId.Value, out var dr)) resp.DropRouteName = dr.Name;

        var stopIds = new[] { e.PickupStopId, e.DropStopId ?? 0 }.Where(x => x > 0).Distinct().ToList();
        var stops = await _db.Set<StopEntity>().AsNoTracking()
            .Where(s => stopIds.Contains(s.Id))
            .ToDictionaryAsync(s => s.Id, s => new { s.Name, s.PickupTime, s.DropTime }, ct);

        if (stops.TryGetValue(e.PickupStopId, out var ps)) { resp.PickupStopName = ps.Name; resp.PickupTime = ps.PickupTime; }
        if (e.DropStopId.HasValue && stops.TryGetValue(e.DropStopId.Value, out var ds)) { resp.DropStopName = ds.Name; resp.DropTime = ds.DropTime; }

        var vehicleIds = new[] { e.PickupVehicleId, e.DropVehicleId ?? 0 }.Where(x => x > 0).Distinct().ToList();
        var vehicles = await _db.Set<VehicleEntity>().AsNoTracking()
            .Where(v => vehicleIds.Contains(v.Id))
            .ToDictionaryAsync(v => v.Id, v => v.VehicleNumber, ct);

        if (vehicles.TryGetValue(e.PickupVehicleId, out var pv)) resp.PickupVehicleNumber = pv;
        if (e.DropVehicleId.HasValue && vehicles.TryGetValue(e.DropVehicleId.Value, out var dv)) resp.DropVehicleNumber = dv;

        return resp;
    }
}
