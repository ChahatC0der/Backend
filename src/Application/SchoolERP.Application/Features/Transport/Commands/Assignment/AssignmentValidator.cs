using Microsoft.EntityFrameworkCore;
using SchoolERP.Domain.Shared.Results;
using SchoolERP.Domain.Transport.Enums;
using RouteEntity = SchoolERP.Domain.Transport.Entities.Route;
using StopEntity = SchoolERP.Domain.Transport.Entities.Stop;
using VehicleEntity = SchoolERP.Domain.Transport.Entities.Vehicle;
using StudentEntity = SchoolERP.Domain.Student.Entities.Student;
using StudentEnrollmentEntity = SchoolERP.Domain.Student.Entities.StudentEnrollment;

namespace SchoolERP.Application.Features.Transport.Commands.Assignment;

internal static class AssignmentValidator
{
    public sealed record ValidatedScope(
        Guid BranchId,   // Student.BranchId is Guid — must match
        string StudentName,
        string EnrollmentId,
        long PickupVehicleCapacity,
        long PickupVehicleId,
        long? DropVehicleId);

    public static async Task<Result<ValidatedScope>> ValidateAsync(
        SchoolERP.Application.Common.Interfaces.IApplicationDbContext db,
        Guid tenantId,
        long studentId,
        long academicYearId,
        long pickupVehicleId,
        long pickupRouteId,
        long pickupStopId,
        long? dropVehicleId,
        long? dropRouteId,
        long? dropStopId,
        CancellationToken ct)
    {
        var student = await db.Set<StudentEntity>().AsNoTracking()
            .Where(s => s.Id == studentId && s.TenantId == tenantId && !s.DeletedAt.HasValue)
            .Select(s => new { s.Id, s.FirstName, s.LastName, s.EnrollmentId, s.BranchId })
            .FirstOrDefaultAsync(ct);
        if (student is null) return Error.NotFound("Student", studentId.ToString());

        var branchId = student.BranchId;   // Guid

        var enrolled = await db.Set<StudentEnrollmentEntity>().AsNoTracking()
            .AnyAsync(e => e.StudentId == studentId && e.AcademicYearId == academicYearId, ct);
        if (!enrolled)
            return Error.Validation($"Student {studentId} is not enrolled for academic year {academicYearId}.");

        var pickupVehicle = await db.Set<VehicleEntity>().AsNoTracking()
            .FirstOrDefaultAsync(v => v.Id == pickupVehicleId && v.BranchId == branchId && !v.IsDeleted, ct);
        if (pickupVehicle is null) return Error.NotFound("Pickup Vehicle", pickupVehicleId.ToString());
        if (pickupVehicle.Status != VehicleStatus.Active)
            return Error.Conflict($"Pickup vehicle {pickupVehicle.VehicleNumber} is {pickupVehicle.Status}. Only Active vehicles allowed.");

        var pickupRoute = await db.Set<RouteEntity>().AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == pickupRouteId && r.BranchId == branchId && !r.IsDeleted, ct);
        if (pickupRoute is null) return Error.NotFound("Pickup Route", pickupRouteId.ToString());
        if (pickupRoute.Status != RouteStatus.Active)
            return Error.Conflict($"Pickup route {pickupRoute.Code} is inactive. Only Active routes allowed.");

        var pickupStop = await db.Set<StopEntity>().AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == pickupStopId && s.RouteId == pickupRouteId && !s.IsDeleted, ct);
        if (pickupStop is null) return Error.NotFound("Pickup Stop", pickupStopId.ToString());
        if (pickupStop.StopType == StopType.DropOnly)
            return Error.Validation($"Stop {pickupStop.Name} is drop-only and cannot be used as pickup.");

        long? effectiveDropVehicleId = dropVehicleId;

        if (dropVehicleId.HasValue || dropRouteId.HasValue || dropStopId.HasValue)
        {
            if (!dropVehicleId.HasValue || !dropRouteId.HasValue || !dropStopId.HasValue)
                return Error.Validation("If any drop field is provided, all three (vehicle, route, stop) must be provided.");

            var dropVehicle = await db.Set<VehicleEntity>().AsNoTracking()
                .FirstOrDefaultAsync(v => v.Id == dropVehicleId.Value && v.BranchId == branchId && !v.IsDeleted, ct);
            if (dropVehicle is null) return Error.NotFound("Drop Vehicle", dropVehicleId.Value.ToString());
            if (dropVehicle.Status != VehicleStatus.Active)
                return Error.Conflict($"Drop vehicle {dropVehicle.VehicleNumber} is {dropVehicle.Status}. Only Active vehicles allowed.");

            var dropRoute = await db.Set<RouteEntity>().AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == dropRouteId.Value && r.BranchId == branchId && !r.IsDeleted, ct);
            if (dropRoute is null) return Error.NotFound("Drop Route", dropRouteId.Value.ToString());
            if (dropRoute.Status != RouteStatus.Active)
                return Error.Conflict($"Drop route {dropRoute.Code} is inactive.");

            var dropStop = await db.Set<StopEntity>().AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == dropStopId.Value && s.RouteId == dropRouteId.Value && !s.IsDeleted, ct);
            if (dropStop is null) return Error.NotFound("Drop Stop", dropStopId.Value.ToString());
            if (dropStop.StopType == StopType.PickupOnly)
                return Error.Validation($"Stop {dropStop.Name} is pickup-only and cannot be used as drop.");
        }

        return Result.Success(new ValidatedScope(
            BranchId: branchId,
            StudentName: $"{student.FirstName} {student.LastName}".Trim(),
            EnrollmentId: student.EnrollmentId,
            PickupVehicleCapacity: pickupVehicle.Capacity,
            PickupVehicleId: pickupVehicleId,
            DropVehicleId: effectiveDropVehicleId));
    }

    public static async Task<Dictionary<long, int>> GetVehicleCurrentCountsAsync(
        SchoolERP.Application.Common.Interfaces.IApplicationDbContext db,
        Guid tenantId,
        long academicYearId,
        IEnumerable<long> vehicleIds,
        CancellationToken ct)
    {
        var idSet = vehicleIds.Distinct().ToList();
        if (idSet.Count == 0) return new();

        var pickupCounts = await db.Set<SchoolERP.Domain.Transport.Entities.TransportAssignment>().AsNoTracking()
            .Where(a => a.TenantId == tenantId && a.AcademicYearId == academicYearId
                     && a.IsCurrent && !a.IsDeleted
                     && idSet.Contains(a.PickupVehicleId))
            .GroupBy(a => a.PickupVehicleId)
            .Select(g => new { VehicleId = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        var dropCounts = await db.Set<SchoolERP.Domain.Transport.Entities.TransportAssignment>().AsNoTracking()
            .Where(a => a.TenantId == tenantId && a.AcademicYearId == academicYearId
                     && a.IsCurrent && !a.IsDeleted
                     && a.DropVehicleId.HasValue
                     && a.DropVehicleId.Value != a.PickupVehicleId
                     && idSet.Contains(a.DropVehicleId.Value))
            .GroupBy(a => a.DropVehicleId!.Value)
            .Select(g => new { VehicleId = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        var result = new Dictionary<long, int>();
        foreach (var p in pickupCounts) result[p.VehicleId] = p.Count;
        foreach (var d in dropCounts) result[d.VehicleId] = result.TryGetValue(d.VehicleId, out var c) ? c + d.Count : d.Count;
        return result;
    }
}
