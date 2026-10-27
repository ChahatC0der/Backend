namespace SchoolERP.Application.Features.Transport.DTOs.Reports;

public sealed class DriverReportRow
{
    public long VehicleId { get; set; }
    public string VehicleNumber { get; set; } = string.Empty;
    public long DriverStaffId { get; set; }
    public DateOnly ValidFrom { get; set; }
    public DateOnly? ValidTo { get; set; }
    public bool IsCurrent { get; set; }
}
