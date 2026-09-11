using SchoolERP.Domain.Common;

namespace SchoolERP.Domain.Student.Entities;

public class StudentDocument : BaseEntity
{
    public long StudentId { get; set; }
    public string DocumentType { get; set; } = string.Empty;    // birth_certificate, aadhar, marksheet, medical, photo, other
    public string FileUrl { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool Verified { get; set; }
    public long? UploadedBy { get; set; }

    // Navigation
    public Student Student { get; set; } = null!;
}
