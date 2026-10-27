namespace SchoolERP.Application.Features.Transport.DTOs.RouteVehicle;

public sealed class AssignVehicleToRouteRequest
{
    public long VehicleId { get; set; }
    public DateOnly ValidFrom { get; set; }
}
