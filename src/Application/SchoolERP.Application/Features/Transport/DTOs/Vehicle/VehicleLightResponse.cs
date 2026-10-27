namespace SchoolERP.Application.Features.Transport.DTOs.Vehicle;

public sealed class VehicleLightResponse
{
    public long Id { get; set; }
    public string VehicleNumber { get; set; } = string.Empty;
    public string VehicleType { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public string Status { get; set; } = string.Empty;
}
