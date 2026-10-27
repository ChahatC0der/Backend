namespace SchoolERP.Application.Features.Transport.DTOs.RouteVehicle;

public sealed class RouteVehicleAssignmentResponse
{
    public long Id { get; set; }
    public long RouteId { get; set; }
    public string? RouteName { get; set; }
    public string? RouteCode { get; set; }
    public long VehicleId { get; set; }
    public string? VehicleNumber { get; set; }
    public DateOnly ValidFrom { get; set; }
    public DateOnly? ValidTo { get; set; }
    public bool IsCurrent { get; set; }
    public DateTime CreatedAt { get; set; }
}
