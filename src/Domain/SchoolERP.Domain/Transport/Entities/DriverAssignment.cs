using SchoolERP.Domain.Common;

namespace SchoolERP.Domain.Transport.Entities;

/// <summary>
/// History-preserving driver-vehicle assignment. One vehicle can have only one current driver
/// (filtered unique index WHERE IsCurrent = 1). DriverStaffId references Users.Id (Staff identity).
/// History via IsCurrent flip; never delete rows.
/// </summary>
public sealed class DriverAssignment : IdEntity
{
    public long VehicleId { get; set; }
    public long DriverStaffId { get; set; }
    public DateOnly ValidFrom { get; set; }
    public DateOnly? ValidTo { get; set; }
    public bool IsCurrent { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
}
