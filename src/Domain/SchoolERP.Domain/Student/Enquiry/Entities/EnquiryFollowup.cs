using SchoolERP.Domain.Common;

namespace SchoolERP.Domain.Student.Enquiry.Entities;

public class EnquiryFollowup : BaseEntity
{
    public long EnquiryId { get; set; }
    public string ActivityType { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateOnly? NextFollowupDate { get; set; }
    public long? CreatedBy { get; set; }

    // Navigation
    public Enquiry Enquiry { get; set; } = null!;
}
