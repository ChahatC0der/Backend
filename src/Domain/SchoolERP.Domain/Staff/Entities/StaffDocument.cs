using SchoolERP.Domain.Common;

namespace SchoolERP.Domain.Staff.Entities;

public class StaffDocument : BaseEntity
{
    public long StaffId { get; set; }
    public string DocumentType { get; set; } = string.Empty;      // aadhar, pan, qualification, etc.
    public string FileUrl { get; set; } = string.Empty;
    public DateOnly? ExpiryDate { get; set; }
    public string VerificationStatus { get; set; } = "not_verified"; // not_verified, verified, rejected
    public long? VerifiedBy { get; set; }
    public DateTime? VerifiedAt { get; set; }

    // Navigation
    public Staff Staff { get; set; } = null!;
}