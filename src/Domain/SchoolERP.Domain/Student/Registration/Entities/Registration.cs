using SchoolERP.Domain.Common;

namespace SchoolERP.Domain.Student.Registration.Entities;

public class Registration : BranchEntity
{
    public Guid TenantId { get; set; }
    public long AcademicYearId { get; set; }
    public long? EnquiryId { get; set; }

    public string RegistrationNumber { get; set; } = string.Empty;

    // Student Details
    public string StudentFirstName { get; set; } = string.Empty;
    public string? StudentLastName { get; set; }
    public DateOnly DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? Birthplace { get; set; }
    public string? Nationality { get; set; }
    public string? MotherTongue { get; set; }
    public string? Religion { get; set; }
    public string? AadharNumber { get; set; }

    // Parents
    public string? FatherName { get; set; }
    public string? FatherMobile { get; set; }
    public string? FatherEmail { get; set; }
    public string? FatherOccupation { get; set; }

    public string? MotherName { get; set; }
    public string? MotherMobile { get; set; }
    public string? MotherEmail { get; set; }
    public string? MotherOccupation { get; set; }

    public string? GuardianName { get; set; }
    public string? GuardianMobile { get; set; }
    public string? GuardianEmail { get; set; }
    public string? GuardianOccupation { get; set; }

    public string ParentPrimaryMobile { get; set; } = string.Empty;
    public string? ParentPrimaryEmail { get; set; }

    // Address
    public string? PresentAddress { get; set; }
    public string? PermanentAddress { get; set; }
    public bool IsPermanentSame { get; set; }

    // Academic
    public long ClassId { get; set; }
    public long? SectionId { get; set; }

    // Previous Education
    public string? PreviousSchool { get; set; }
    public string? PreviousBoard { get; set; }
    public decimal? PreviousPercentage { get; set; }
    public int? PreviousYearPassing { get; set; }
    public string? TransferCertificateUrl { get; set; }

    // Additional
    public string? Source { get; set; }
    public string? MedicalConditions { get; set; }
    public string? CustomFields { get; set; }

    // Registration Fee
    public bool RegistrationFeePaid { get; set; }
    public decimal? RegistrationFeeAmount { get; set; }
    public DateOnly? RegistrationFeePaymentDate { get; set; }
    public string? RegistrationFeePaymentMode { get; set; }

    // Interview
    public DateTime? InterviewDate { get; set; }
    public string? InterviewMode { get; set; }
    public long? InterviewerId { get; set; }
    public string? InterviewResult { get; set; }
    public string? InterviewFeedback { get; set; }

    // Approval
    public DateTime? ApprovedAt { get; set; }
    public long? ApprovedBy { get; set; }
    public string? RejectionReason { get; set; }

    // Parent Account
    public long? ParentUserId { get; set; }

    // Conversion
    public long? ConvertedToStudentId { get; set; }

    // Status
    public string Status { get; set; } = "draft";

    // Navigation
    public ICollection<RegistrationDocument> Documents { get; set; } = new List<RegistrationDocument>();
    public ICollection<RegistrationAuditLog> AuditLogs { get; set; } = new List<RegistrationAuditLog>();
}
