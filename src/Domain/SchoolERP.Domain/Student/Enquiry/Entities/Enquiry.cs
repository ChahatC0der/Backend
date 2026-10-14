using SchoolERP.Domain.Common;

namespace SchoolERP.Domain.Student.Enquiry.Entities;

public class Enquiry : BranchEntity
{
    public DateTime EnquiryDate { get; set; } = DateTime.UtcNow;
    public string Source { get; set; } = string.Empty;
    public string? StudentFirstName { get; set; }
    public string? StudentLastName { get; set; }
    public string ParentName { get; set; } = string.Empty;
    public string Mobile { get; set; } = string.Empty;
    public string? Email { get; set; }
    public long? ClassId { get; set; }
    public string? Message { get; set; }
    public long? AssignedTo { get; set; }
    public string Status { get; set; } = "new";
    public long? ConvertedToRegistrationId { get; set; }

    // Navigation
    public ICollection<EnquiryFollowup> Followups { get; set; } = new List<EnquiryFollowup>();
    public ICollection<EnquiryStatusLog> StatusLogs { get; set; } = new List<EnquiryStatusLog>();
}
