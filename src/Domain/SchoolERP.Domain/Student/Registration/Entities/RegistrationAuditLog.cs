using SchoolERP.Domain.Common;

namespace SchoolERP.Domain.Student.Registration.Entities;

public class RegistrationAuditLog : BaseEntity
{
    public long RegistrationId { get; set; }
    public string? OldStatus { get; set; }
    public string NewStatus { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public long? PerformedBy { get; set; }

    // Navigation
    public Registration Registration { get; set; } = null!;
}
