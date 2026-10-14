using SchoolERP.Domain.Common;

namespace SchoolERP.Domain.Student.Registration.Entities;

public class RegistrationDocument : BaseEntity
{
    public long RegistrationId { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public string? FileUrl { get; set; }
    public string? Description { get; set; }
    public string Status { get; set; } = "not_uploaded";
    public string? RejectionReason { get; set; }
    public long? VerifiedBy { get; set; }
    public DateTime? VerifiedAt { get; set; }

    // Navigation
    public Registration Registration { get; set; } = null!;
}
