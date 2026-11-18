using TransportAssignmentEntity = SchoolERP.Domain.Transport.Entities.TransportAssignment;

namespace SchoolERP.Application.Features.Transport.DTOs.Assignment;

public sealed class TransportAssignmentResponse
{
    public long Id { get; set; }
    public long StudentId { get; set; }
    public string? StudentName { get; set; }
    public string? EnrollmentId { get; set; }
    public long AcademicYearId { get; set; }

    public long PickupVehicleId { get; set; }
    public string? PickupVehicleNumber { get; set; }
    public long PickupRouteId { get; set; }
    public string? PickupRouteName { get; set; }
    public string? PickupRouteCode { get; set; }
    public long PickupStopId { get; set; }
    public string? PickupStopName { get; set; }
    public TimeOnly? PickupTime { get; set; }

    public long? DropVehicleId { get; set; }
    public string? DropVehicleNumber { get; set; }
    public long? DropRouteId { get; set; }
    public string? DropRouteName { get; set; }
    public long? DropStopId { get; set; }
    public string? DropStopName { get; set; }
    public TimeOnly? DropTime { get; set; }

    public DateOnly ValidFrom { get; set; }
    public DateOnly? ValidTo { get; set; }
    public bool IsCurrent { get; set; }
    public DateTime CreatedAt { get; set; }

    /// <summary>Capacity warnings (empty if none). Non-blocking — response succeeds with warnings.</summary>
    public List<string> Warnings { get; set; } = new();


    public static TransportAssignmentResponse FromEntity(TransportAssignmentEntity a) => new()
    {
        Id = a.Id,
        StudentId = a.StudentId,
        StudentName = a.Student != null
            ? $"{a.Student.FirstName} {a.Student.LastName}".Trim()
            : null,

        // EnrollmentId will be mapped after verifying
        // the Student enrollment relationship.
        //EnrollmentId = a.Student,

        AcademicYearId = a.AcademicYearId,

        PickupVehicleId = a.PickupVehicleId,
        PickupVehicleNumber = a.PickupVehicle?.VehicleNumber,

        PickupRouteId = a.PickupRouteId,
        PickupRouteName = a.PickupRoute?.Name,
        PickupRouteCode = a.PickupRoute?.Code,

        PickupStopId = a.PickupStopId,
        PickupStopName = a.PickupStop?.Name,

        PickupTime = a.PickupStop?.PickupTime,

        DropVehicleId = a.DropVehicleId,
        DropVehicleNumber = a.DropVehicle?.VehicleNumber,

        DropRouteId = a.DropRouteId,
        DropRouteName = a.DropRoute?.Name,
        DropStopId = a.DropStopId,
        DropStopName = a.DropStop?.Name,

        DropTime = a.DropStop?.DropTime,

        ValidFrom = a.ValidFrom,
        ValidTo = a.ValidTo,
        IsCurrent = a.IsCurrent,
        CreatedAt = a.CreatedAt,
    };

}
