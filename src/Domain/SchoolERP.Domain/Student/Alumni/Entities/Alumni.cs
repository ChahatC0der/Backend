using SchoolERP.Domain.Common;

namespace SchoolERP.Domain.Student.Alumni.Entities;

public class Alumni : BranchEntity, IMustHaveTenant
{
    public Guid TenantId { get; set; }
    public long? StudentId { get; set; }
    public string AlumniId { get; set; } = string.Empty;

    // Snapshot at graduation
    public string FirstName { get; set; } = string.Empty;
    public string? LastName { get; set; }
    public DateOnly DateOfBirth { get; set; }
    public string? Gender { get; set; }

    // Graduation details
    public DateOnly GraduationDate { get; set; }
    public long LastClassId { get; set; }
    public long PassingAcademicYearId { get; set; }
    public decimal? FinalPercentage { get; set; }

    public string CurrentStatus { get; set; } = "not_employed";

    // Profile
    public string? Occupation { get; set; }
    public string? CompanyName { get; set; }
    public string? Industry { get; set; }

    public string? HigherEducationInstitution { get; set; }
    public string? HigherEducationCourse { get; set; }
    public int? HigherEducationYear { get; set; }

    public string? ContactEmail { get; set; }
    public string? ContactMobile { get; set; }
    public string? Address { get; set; }

    public string? SocialLinks { get; set; }             // JSON
    public string? PhotoUrl { get; set; }

    public string PrivacySettings { get; set; } = "{}";           // JSON
    public string CommunicationPreferences { get; set; } = "{}";  // JSON
    public string? Tags { get; set; }                     // JSON

    public DateOnly? LastInteractionDate { get; set; }
    public bool IsDeceased { get; set; }
    public bool IsArchived { get; set; }

    // Navigation
    public ICollection<AlumniDonation> Donations { get; set; } = new List<AlumniDonation>();
    public ICollection<AlumniSuccessStory> SuccessStories { get; set; } = new List<AlumniSuccessStory>();
    public ICollection<AlumniReferral> Referrals { get; set; } = new List<AlumniReferral>();
    public ICollection<AlumniEventRegistration> EventRegistrations { get; set; } = new List<AlumniEventRegistration>();
}
