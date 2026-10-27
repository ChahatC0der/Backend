using SchoolERP.Domain.Common;
using SchoolERP.Domain.Transport.Enums;

namespace SchoolERP.Domain.Transport.Entities;

/// <summary>
/// Vehicle belongs to one branch. VehicleNumber unique per (TenantId, BranchId).
/// Soft delete via IsDeleted (physical). No DeletedAt column in DB.
/// </summary>
public sealed class Vehicle : BaseBranchEntity
{
    public string VehicleNumber { get; set; } = string.Empty;
    public VehicleType VehicleType { get; set; }
    public int Capacity { get; set; }
    public string? Model { get; set; }
    public string? Make { get; set; }
    public int? Year { get; set; }
    public DateOnly? InsuranceValidUntil { get; set; }
    public DateOnly? PucValidUntil { get; set; }
    public DateOnly? FitnessValidUntil { get; set; }
    public VehicleStatus Status { get; set; } = VehicleStatus.Active;
}
