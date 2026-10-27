namespace SchoolERP.Application.Features.Transport.DTOs.Assignment;

/// <summary>Same shape as Create. Standalone — do NOT inherit from sealed CreateTransportAssignmentRequest.</summary>
public sealed class ChangeTransportAssignmentRequest
{
    public long StudentId { get; set; }
    public long AcademicYearId { get; set; }

    public long PickupVehicleId { get; set; }
    public long PickupRouteId { get; set; }
    public long PickupStopId { get; set; }

    public long? DropVehicleId { get; set; }
    public long? DropRouteId { get; set; }
    public long? DropStopId { get; set; }

    public DateOnly ValidFrom { get; set; }

    public bool OverrideCapacity { get; set; }
    public string? OverrideReason { get; set; }
}
