using SchoolERP.Domain.Common;

namespace SchoolERP.Domain.Student.Entities;

public class Student : BranchEntity
{
    public Guid TenantId { get; set; }
    public long? UserId { get; set; }
    public long? FamilyId { get; set; }                     // 🔥 NEW: link to family (siblings)

    public string EnrollmentId { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;
    public string? LastName { get; set; }
    public DateOnly DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? BloodGroup { get; set; }
    public string? Nationality { get; set; }
    public string? Religion { get; set; }
    public string? MotherTongue { get; set; }

    public string? PersonalEmail { get; set; }
    public string? PersonalMobile { get; set; }
    public string? AlternateMobile { get; set; }
    public string? Address { get; set; }

    public DateOnly AdmissionDate { get; set; }
    public string? RollNumber { get; set; }

    public string Status { get; set; } = "enrolled";

    public string? PhotoUrl { get; set; }
    public string? CustomFields { get; set; }

    // Navigation
    public Family? Family { get; set; }                     // 🔥 NEW
    public ICollection<StudentEnrollment> Enrollments { get; set; } = new List<StudentEnrollment>();
    public ICollection<Parent> Parents { get; set; } = new List<Parent>();
    public ICollection<StudentDocument> Documents { get; set; } = new List<StudentDocument>();
}
