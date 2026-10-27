namespace SchoolERP.Application.Features.Transport.DTOs.Vehicle;

public sealed class PatchVehicleStatusRequest
{
    public string Status { get; set; } = string.Empty;   // active | maintenance | retired
}
