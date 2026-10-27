namespace SchoolERP.Application.Features.Transport.DTOs.Portal;

/// <summary>
/// Parent-facing view of a child's transport. No fee amount exposed.
/// Null response means no active transport assignment.
/// </summary>
public sealed class ParentTransportResponse
{
    public long StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string EnrollmentId { get; set; } = string.Empty;
    public long AcademicYearId { get; set; }

    public string? PickupRouteName { get; set; }
    public string? PickupRouteCode { get; set; }
    public string? PickupStopName { get; set; }
    public string? PickupLandmark { get; set; }
    public string? PickupTime { get; set; }        // formatted "HH:mm"
    public string? PickupVehicleNumber { get; set; }

    public string? DropRouteName { get; set; }
    public string? DropStopName { get; set; }
    public string? DropTime { get; set; }
    public string? DropVehicleNumber { get; set; }

    public DateOnly ValidFrom { get; set; }
    public DateOnly? ValidTo { get; set; }
}
