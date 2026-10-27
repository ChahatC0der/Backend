namespace SchoolERP.Application.Features.Transport.DTOs.VehicleDocument;

public sealed class UploadVehicleDocumentRequest
{
    public string DocumentType { get; set; } = string.Empty;  // insurance | puc | fitness | registration
    public string FileUrl { get; set; } = string.Empty;       // external URL — frontend uploads via S3/storage first
    public DateOnly? ExpiryDate { get; set; }
}
