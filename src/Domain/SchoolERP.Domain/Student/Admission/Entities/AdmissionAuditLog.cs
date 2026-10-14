using SchoolERP.Domain.Common;

namespace SchoolERP.Domain.Student.Admission.Entities;

public class AdmissionAuditLog : BaseEntity
{
    public long AdmissionId { get; set; }
    public string? OldStatus { get; set; }
    public string NewStatus { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public long? PerformedBy { get; set; }

    // Navigation
    public Admission Admission { get; set; } = null!;
}
