namespace SchoolERP.Application.Features.Transport.DTOs.Vehicle;

public sealed class VehicleExportRow
{
    public long Id { get; set; }
    public string VehicleNumber { get; set; } = string.Empty;
    public string VehicleType { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public string? Make { get; set; }
    public string? Model { get; set; }
    public int? Year { get; set; }
    public DateOnly? InsuranceValidUntil { get; set; }
    public DateOnly? PucValidUntil { get; set; }
    public DateOnly? FitnessValidUntil { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
