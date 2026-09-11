using SchoolERP.Domain.Common;

namespace SchoolERP.Domain.Staff.Entities;

public class Staff : BranchEntity
{
    public Guid TenantId { get; set; }                  // extra column (DDL me hai)
    public long? UserId { get; set; }
    public long? DepartmentId { get; set; }

    public string StaffId { get; set; } = string.Empty; // auto-generated

    // Personal
    public string FirstName { get; set; } = string.Empty;
    public string? LastName { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? BloodGroup { get; set; }
    public string? MaritalStatus { get; set; }
    public string? Nationality { get; set; }

    // Contact
    public string? PersonalEmail { get; set; }
    public string? WorkEmail { get; set; }
    public string? PersonalMobile { get; set; }
    public string? WorkMobile { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }

    // Address
    public string? PresentAddress { get; set; }
    public string? PermanentAddress { get; set; }

    // Professional
    public DateOnly JoiningDate { get; set; }
    public string EmploymentType { get; set; } = string.Empty;   // permanent, contract, probation, intern
    public string StaffType { get; set; } = string.Empty;        // teaching, non_teaching
    public string Designation { get; set; } = string.Empty;

    // Academic
    public string? Qualification { get; set; }
    public string? Specialization { get; set; }
    public decimal? ExperienceYears { get; set; }
    public string? PreviousInstitution { get; set; }

    public string? PhotoUrl { get; set; }
    public string Status { get; set; } = "active";               // active, inactive, suspended, terminated
    public string? CustomFields { get; set; }                     // JSON

    // Navigation
    public Department? Department { get; set; }
    public ICollection<StaffDocument> Documents { get; set; } = new List<StaffDocument>();
}