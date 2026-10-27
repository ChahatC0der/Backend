namespace SchoolERP.Application.Features.Transport.DTOs.Assignment;

public sealed class CreateTransportAssignmentRequest
{
    public long StudentId { get; set; }
    public long AcademicYearId { get; set; }

    public long PickupVehicleId { get; set; }
    public long PickupRouteId { get; set; }
    public long PickupStopId { get; set; }

    // Omit or leave null to use same as pickup
    public long? DropVehicleId { get; set; }
    public long? DropRouteId { get; set; }
    public long? DropStopId { get; set; }

    public DateOnly ValidFrom { get; set; }

    /// <summary>If true, admin accepts capacity overflow warning.</summary>
    public bool OverrideCapacity { get; set; }
    public string? OverrideReason { get; set; }
}
