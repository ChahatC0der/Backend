using VehicleMaintenanceEntity = SchoolERP.Domain.Transport.Entities.VehicleMaintenance;

namespace SchoolERP.Application.Features.Transport.DTOs.VehicleMaintenance;

public sealed class VehicleMaintenanceResponse
{
    public long Id { get; set; }
    public long VehicleId { get; set; }
    public DateOnly MaintenanceDate { get; set; }
    public string? Description { get; set; }
    public decimal? Cost { get; set; }
    public long? OdometerReading { get; set; }
    public DateTime CreatedAt { get; set; }

    public static VehicleMaintenanceResponse FromEntity(VehicleMaintenanceEntity m) => new()
    {
        Id = m.Id,
        VehicleId = m.VehicleId,
        MaintenanceDate = m.MaintenanceDate,
        Description = m.Description,
        Cost = m.Cost,
        OdometerReading = m.OdometerReading,
        CreatedAt = m.CreatedAt,
    };
}
