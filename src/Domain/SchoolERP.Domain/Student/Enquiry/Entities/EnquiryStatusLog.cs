using SchoolERP.Domain.Common;

namespace SchoolERP.Domain.Student.Enquiry.Entities;

public class EnquiryStatusLog : BaseEntity
{
    public long EnquiryId { get; set; }
    public string? OldStatus { get; set; }
    public string NewStatus { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public long? PerformedBy { get; set; }

    // Navigation
    public Enquiry Enquiry { get; set; } = null!;
}
