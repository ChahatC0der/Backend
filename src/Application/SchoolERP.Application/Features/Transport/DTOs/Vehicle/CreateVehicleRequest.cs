namespace SchoolERP.Application.Features.Transport.DTOs.Vehicle;

public sealed class CreateVehicleRequest
{
    public string VehicleNumber { get; set; } = string.Empty;
    public string VehicleType { get; set; } = "bus";
    public int Capacity { get; set; }
    public string? Model { get; set; }
    public string? Make { get; set; }
    public int? Year { get; set; }
    public DateOnly? InsuranceValidUntil { get; set; }
    public DateOnly? PucValidUntil { get; set; }
    public DateOnly? FitnessValidUntil { get; set; }
}
