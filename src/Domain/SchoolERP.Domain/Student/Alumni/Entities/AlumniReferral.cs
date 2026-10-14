using SchoolERP.Domain.Common;

namespace SchoolERP.Domain.Student.Alumni.Entities;

public class AlumniReferral : BranchEntity
{
    public long AlumniId { get; set; }
    public string ReferredName { get; set; } = string.Empty;
    public string ReferredContact { get; set; } = string.Empty;
    public string? ReferredEmail { get; set; }
    public long? ReferredClassId { get; set; }
    public string Status { get; set; } = "pending";
    public long? EnquiryId { get; set; }

    public Alumni Alumni { get; set; } = null!;
}
