using SchoolERP.Domain.Common;

namespace SchoolERP.Domain.Transport.Entities;

/// <summary>
/// History-preserving route-vehicle junction. One vehicle can be current on only one route
/// (filtered unique index WHERE IsCurrent = 1). One route can have multiple current vehicles (N:M).
/// History preserved by flipping IsCurrent = 0 and setting ValidTo; never delete rows.
/// </summary>
public sealed class RouteVehicle : IdEntity
{
    public long RouteId { get; set; }
    public long VehicleId { get; set; }
    public DateOnly ValidFrom { get; set; }
    public DateOnly? ValidTo { get; set; }
    public bool IsCurrent { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
}
