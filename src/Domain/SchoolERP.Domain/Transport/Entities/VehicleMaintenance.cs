using SchoolERP.Domain.Common;

namespace SchoolERP.Domain.Transport.Entities;

/// <summary>
/// Append-only maintenance log — no update or delete endpoints exposed.
/// Scoped through Vehicle (no TenantId/BranchId columns).
/// </summary>
public sealed class VehicleMaintenance : IdEntity
{
    public long VehicleId { get; set; }
    public DateOnly MaintenanceDate { get; set; }
    public string? Description { get; set; }
    public decimal? Cost { get; set; }
    public long? OdometerReading { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; }
}
