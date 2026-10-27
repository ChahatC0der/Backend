namespace SchoolERP.Application.Features.Transport.DTOs.Assignment;

public sealed class BulkTransportAssignmentCsvRow
{
    public long StudentId { get; set; }
    public long PickupVehicleId { get; set; }
    public long PickupRouteId { get; set; }
    public long PickupStopId { get; set; }
    public long? DropVehicleId { get; set; }
    public long? DropRouteId { get; set; }
    public long? DropStopId { get; set; }
    public DateOnly? ValidFrom { get; set; }   // null = today
}

/// <summary>Cap 500 rows. All-or-nothing per request.</summary>
public sealed class BulkTransportAssignmentCsvRequest
{
    public long AcademicYearId { get; set; }
    public List<BulkTransportAssignmentCsvRow> Rows { get; set; } = new();
    public bool OverrideCapacity { get; set; }
}
