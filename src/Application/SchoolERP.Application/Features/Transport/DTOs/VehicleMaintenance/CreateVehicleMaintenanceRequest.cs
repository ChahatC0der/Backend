namespace SchoolERP.Application.Features.Transport.DTOs.VehicleMaintenance;

public sealed class CreateVehicleMaintenanceRequest
{
    public DateOnly MaintenanceDate { get; set; }
    public string? Description { get; set; }
    public decimal? Cost { get; set; }
    public long? OdometerReading { get; set; }
}
