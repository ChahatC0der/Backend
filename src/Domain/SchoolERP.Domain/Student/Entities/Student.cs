using SchoolERP.Domain.Common;

namespace SchoolERP.Domain.Student.Entities;

public class Student : BranchEntity
{
    public Guid TenantId { get; set; }                          // extra (DDL me hai, base me nahi)
    public long? UserId { get; set; }                           // login link (optional)

    public string EnrollmentId { get; set; } = string.Empty;    // BRANCH-2025-00123

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
    // registered, enrolled, active, graduated, dropped, blocked, transferred_out

    public string? PhotoUrl { get; set; }
    public string? CustomFields { get; set; }                   // JSON

    // Navigation
    public ICollection<StudentEnrollment> Enrollments { get; set; } = new List<StudentEnrollment>();
    public ICollection<Parent> Parents { get; set; } = new List<Parent>();
    public ICollection<StudentDocument> Documents { get; set; } = new List<StudentDocument>();
}
