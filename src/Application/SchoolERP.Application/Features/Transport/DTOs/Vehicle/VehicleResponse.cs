using VehicleEntity = SchoolERP.Domain.Transport.Entities.Vehicle;

namespace SchoolERP.Application.Features.Transport.DTOs.Vehicle;

public sealed class VehicleResponse
{
    public long Id { get; set; }
    public string VehicleNumber { get; set; } = string.Empty;
    public string VehicleType { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public string? Model { get; set; }
    public string? Make { get; set; }
    public int? Year { get; set; }
    public DateOnly? InsuranceValidUntil { get; set; }
    public DateOnly? PucValidUntil { get; set; }
    public DateOnly? FitnessValidUntil { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public static VehicleResponse FromEntity(VehicleEntity v) => new()
    {
        Id = v.Id,
        VehicleNumber = v.VehicleNumber,
        VehicleType = v.VehicleType.ToString(),
        Capacity = v.Capacity,
        Model = v.Model,
        Make = v.Make,
        Year = v.Year,
        InsuranceValidUntil = v.InsuranceValidUntil,
        PucValidUntil = v.PucValidUntil,
        FitnessValidUntil = v.FitnessValidUntil,
        Status = v.Status.ToString(),
        CreatedAt = v.CreatedAt,
        UpdatedAt = v.UpdatedAt,
    };
}
