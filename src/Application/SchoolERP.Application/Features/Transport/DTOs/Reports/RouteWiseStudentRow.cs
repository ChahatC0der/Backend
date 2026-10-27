namespace SchoolERP.Application.Features.Transport.DTOs.Reports;

public sealed class RouteWiseStudentRow
{
    public long RouteId { get; set; }
    public string RouteName { get; set; } = string.Empty;
    public string RouteCode { get; set; } = string.Empty;
    public long StopId { get; set; }
    public string StopName { get; set; } = string.Empty;
    public int StopOrder { get; set; }
    public TimeOnly? PickupTime { get; set; }
    public string? VehicleNumber { get; set; }
    public long StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string EnrollmentId { get; set; } = string.Empty;
    public string? ParentMobile { get; set; }
}
