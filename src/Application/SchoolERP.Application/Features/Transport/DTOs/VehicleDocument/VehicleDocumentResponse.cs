using VehicleDocumentEntity = SchoolERP.Domain.Transport.Entities.VehicleDocument;

namespace SchoolERP.Application.Features.Transport.DTOs.VehicleDocument;

public sealed class VehicleDocumentResponse
{
    public long Id { get; set; }
    public long VehicleId { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public DateOnly? ExpiryDate { get; set; }
    public long? UploadedBy { get; set; }
    public DateTime CreatedAt { get; set; }

    public static VehicleDocumentResponse FromEntity(VehicleDocumentEntity d) => new()
    {
        Id = d.Id,
        VehicleId = d.VehicleId,
        DocumentType = d.DocumentType.ToString(),
        FileUrl = d.FileUrl,
        ExpiryDate = d.ExpiryDate,
        UploadedBy = d.UploadedBy,
        CreatedAt = d.CreatedAt,
    };
}
