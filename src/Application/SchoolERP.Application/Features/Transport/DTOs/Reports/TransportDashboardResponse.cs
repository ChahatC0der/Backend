namespace SchoolERP.Application.Features.Transport.DTOs.Reports;

public sealed class TransportDashboardResponse
{
    public int VehicleTotal { get; set; }
    public int VehicleActive { get; set; }
    public int VehicleMaintenance { get; set; }
    public int VehicleRetired { get; set; }
    public int RouteTotal { get; set; }
    public int RouteActive { get; set; }
    public int CurrentAssignments { get; set; }
    public int ExpiringDocumentsCount { get; set; }
    public int ExpiredDocumentsCount { get; set; }
}
