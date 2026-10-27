using SchoolERP.Domain.Common;

namespace SchoolERP.Domain.Transport.Entities;

/// <summary>
/// Student transport assignment — pickup and drop can be same or different route/stop/vehicle.
/// Has TenantId but no BranchId (validate branch via Student).
/// One current per (StudentId, AcademicYearId) via filtered unique index.
/// Null drop IDs mean "same as pickup" per schema comment.
/// </summary>
public sealed class TransportAssignment : BaseTenantEntity
{
    public long StudentId { get; set; }
    public long AcademicYearId { get; set; }

    // Pickup (required)
    public long PickupVehicleId { get; set; }
    public long PickupRouteId { get; set; }
    public long PickupStopId { get; set; }

    // Drop (null = same as pickup)
    public long? DropVehicleId { get; set; }
    public long? DropRouteId { get; set; }
    public long? DropStopId { get; set; }

    public DateOnly ValidFrom { get; set; }
    public DateOnly? ValidTo { get; set; }
    public bool IsCurrent { get; set; }
}
