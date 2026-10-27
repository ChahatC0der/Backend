namespace SchoolERP.Application.Features.Transport.DTOs.Reports;

public sealed class VehicleMaintenanceReportRow
{
    public long VehicleId { get; set; }
    public string VehicleNumber { get; set; } = string.Empty;
    public DateOnly MaintenanceDate { get; set; }
    public string? Description { get; set; }
    public decimal? Cost { get; set; }
    public long? OdometerReading { get; set; }
}
