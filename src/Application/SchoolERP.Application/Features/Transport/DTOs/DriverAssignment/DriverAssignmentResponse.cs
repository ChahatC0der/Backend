namespace SchoolERP.Application.Features.Transport.DTOs.DriverAssignment;

public sealed class DriverAssignmentResponse
{
    public long Id { get; set; }
    public long VehicleId { get; set; }
    public string? VehicleNumber { get; set; }
    public long DriverStaffId { get; set; }
    public string? DriverName { get; set; }
    public DateOnly ValidFrom { get; set; }
    public DateOnly? ValidTo { get; set; }
    public bool IsCurrent { get; set; }
    public DateTime CreatedAt { get; set; }
}
