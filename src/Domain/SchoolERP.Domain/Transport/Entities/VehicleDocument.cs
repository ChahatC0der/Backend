using SchoolERP.Domain.Common;
using SchoolERP.Domain.Transport.Enums;

namespace SchoolERP.Domain.Transport.Entities;

/// <summary>
/// Vehicle document (insurance, PUC, fitness, registration).
/// Scoped through Vehicle (no TenantId/BranchId columns).
/// Unique (VehicleId, DocumentType) enforced at DB level.
/// File upload happens externally — this row stores FileUrl pointer.
/// </summary>
public sealed class VehicleDocument : IdEntity
{
    public long VehicleId { get; set; }
    public VehicleDocumentType DocumentType { get; set; }
    public string FileUrl { get; set; } = string.Empty;
    public DateOnly? ExpiryDate { get; set; }
    public long? UploadedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; }
}
