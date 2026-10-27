namespace SchoolERP.Application.Features.Transport.DTOs.Reports;

public sealed class StudentTransportReportRow
{
    public long StudentId { get; set; }
    public string EnrollmentId { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public string? ParentMobile { get; set; }
    public string? PickupRouteName { get; set; }
    public string? PickupStopName { get; set; }
    public TimeOnly? PickupTime { get; set; }
    public string? PickupVehicleNumber { get; set; }
    public string? DropRouteName { get; set; }
    public string? DropStopName { get; set; }
    public TimeOnly? DropTime { get; set; }
    public string? DropVehicleNumber { get; set; }
}
