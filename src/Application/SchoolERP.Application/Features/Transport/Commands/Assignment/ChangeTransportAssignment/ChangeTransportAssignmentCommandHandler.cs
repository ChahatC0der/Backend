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

namespace SchoolERP.Application.Features.Transport.Commands.Assignment.ChangeTransportAssignment;

public sealed class ChangeTransportAssignmentCommandHandler : IRequestHandler<ChangeTransportAssignmentCommand, Result<TransportAssignmentResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;

    public ChangeTransportAssignmentCommandHandler(IApplicationDbContext db, ICurrentTenantService tenant)
    { _db = db; _tenant = tenant; }

    public async Task<Result<TransportAssignmentResponse>> Handle(ChangeTransportAssignmentCommand request, CancellationToken ct)
    {
        var tenantId = _tenant.GetTenantId();
        if (tenantId == Guid.Empty) return Error.Unauthorized("Tenant context is not resolved.");

        var r = request.Request;

        // Validate structure
        var validation = await AssignmentValidator.ValidateAsync(_db, tenantId, r.StudentId, r.AcademicYearId,
            r.PickupVehicleId, r.PickupRouteId, r.PickupStopId,
            r.DropVehicleId, r.DropRouteId, r.DropStopId, ct);
        if (validation.IsFailure) return validation.Error!;
        var scope = validation.Value;

        // Find current assignment
        var current = await _db.Set<TransportAssignmentEntity>().FirstOrDefaultAsync(a =>
            a.TenantId == tenantId && a.StudentId == r.StudentId && a.AcademicYearId == r.AcademicYearId
            && a.IsCurrent && !a.IsDeleted, ct);
        if (current is null)
            return Error.NotFound("Current TransportAssignment", $"{r.StudentId}/{r.AcademicYearId}");

        if (r.ValidFrom <= current.ValidFrom)
            return Error.Validation($"New ValidFrom ({r.ValidFrom:yyyy-MM-dd}) must be after current assignment ValidFrom ({current.ValidFrom:yyyy-MM-dd}).");

        // Capacity warnings
        var warnings = new List<string>();
        var counts = await AssignmentValidator.GetVehicleCurrentCountsAsync(_db, tenantId, r.AcademicYearId,
            new[] { r.PickupVehicleId }, ct);
        var pc = counts.TryGetValue(r.PickupVehicleId, out var c) ? c : 0;
        if (pc + 1 > scope.PickupVehicleCapacity)
            warnings.Add($"Pickup vehicle capacity exceeded: current {pc}, capacity {scope.PickupVehicleCapacity}.");

        if (warnings.Count > 0 && !r.OverrideCapacity)
            return Error.Conflict("Capacity exceeded. Set OverrideCapacity = true to proceed. Warnings: " + string.Join(" | ", warnings));

        if (r.DropRouteId.HasValue && r.DropRouteId.Value != r.PickupRouteId)
            warnings.Add("Pickup and drop routes differ — notify the transport manager.");

        // Close previous
        current.IsCurrent = false;
        current.ValidTo = r.ValidFrom.AddDays(-1);
        current.UpdatedAt = DateTime.UtcNow;

        // Insert new
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

        // TODO (audit): await _auditWriter.LogAsync("TransportAssignments", entity.Id, FinancialAuditAction.Update, current, entity, ct);

        var resp = TransportAssignmentResponse.FromEntity(entity);
        resp.Warnings = warnings;
        return Result.Success(resp);
    }
}
