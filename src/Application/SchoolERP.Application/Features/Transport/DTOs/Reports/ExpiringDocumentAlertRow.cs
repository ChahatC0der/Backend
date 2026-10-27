namespace SchoolERP.Application.Features.Transport.DTOs.Reports;

public sealed class ExpiringDocumentAlertRow
{
    public long VehicleId { get; set; }
    public string VehicleNumber { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;
    public DateOnly ExpiryDate { get; set; }
    public int DaysUntilExpiry { get; set; }
    public bool IsExpired { get; set; }
}
