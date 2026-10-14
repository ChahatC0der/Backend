using SchoolERP.Application.Common.DTOs;

namespace SchoolERP.Application.Features.Student.DTOs;

// ==================== ALUMNI ====================

public record CreateAlumniRequest(
    long? StudentId,
    string FirstName,
    string? LastName,
    DateOnly DateOfBirth,
    DateOnly GraduationDate,
    long LastClassId,
    long PassingAcademicYearId,
    string? Gender = null,
    decimal? FinalPercentage = null,
    string CurrentStatus = "not_employed",
    string? Occupation = null,
    string? CompanyName = null,
    string? Industry = null,
    string? HigherEducationInstitution = null,
    string? HigherEducationCourse = null,
    int? HigherEducationYear = null,
    string? ContactEmail = null,
    string? ContactMobile = null,
    string? Address = null,
    string? SocialLinks = null,
    string? PhotoUrl = null,
    string PrivacySettings = "{}",
    string CommunicationPreferences = "{}",
    string? Tags = null
);

public record UpdateAlumniRequest(
    long Id,
    string FirstName,
    string? LastName,
    DateOnly DateOfBirth,
    string? Gender,
    DateOnly GraduationDate,
    long LastClassId,
    long PassingAcademicYearId,
    decimal? FinalPercentage,
    string CurrentStatus,
    string? Occupation,
    string? CompanyName,
    string? Industry,
    string? HigherEducationInstitution,
    string? HigherEducationCourse,
    int? HigherEducationYear,
    string? ContactEmail,
    string? ContactMobile,
    string? Address,
    string? SocialLinks,
    string? PhotoUrl,
    string PrivacySettings,
    string CommunicationPreferences,
    string? Tags,
    bool IsDeceased,
    bool IsArchived
);

public record PatchAlumniRequest(
    long Id,
    string? FirstName = null,
    string? LastName = null,
    DateOnly? DateOfBirth = null,
    string? Gender = null,
    DateOnly? GraduationDate = null,
    long? LastClassId = null,
    long? PassingAcademicYearId = null,
    decimal? FinalPercentage = null,
    string? CurrentStatus = null,
    string? Occupation = null,
    string? CompanyName = null,
    string? Industry = null,
    string? HigherEducationInstitution = null,
    string? HigherEducationCourse = null,
    int? HigherEducationYear = null,
    string? ContactEmail = null,
    string? ContactMobile = null,
    string? Address = null,
    string? SocialLinks = null,
    string? PhotoUrl = null,
    string? PrivacySettings = null,
    string? CommunicationPreferences = null,
    string? Tags = null,
    bool? IsDeceased = null,
    bool? IsArchived = null
);

public record UpdateAlumniProfileRequest(
    long Id,
    string? Occupation = null,
    string? CompanyName = null,
    string? Industry = null,
    string? HigherEducationInstitution = null,
    string? HigherEducationCourse = null,
    int? HigherEducationYear = null,
    string? ContactEmail = null,
    string? ContactMobile = null,
    string? Address = null,
    string? SocialLinks = null,
    string? PhotoUrl = null,
    string? PrivacySettings = null,
    string? CommunicationPreferences = null
);

public record ConvertStudentToAlumniRequest(
    long StudentId,
    DateOnly GraduationDate,
    long LastClassId,
    long PassingAcademicYearId,
    decimal? FinalPercentage = null
);

public record AlumniResponse(
    long Id,
    Guid TenantId,
    Guid BranchId,
    long? StudentId,
    string AlumniId,
    string FirstName,
    string? LastName,
    string FullName,
    DateOnly DateOfBirth,
    string? Gender,
    DateOnly GraduationDate,
    long LastClassId,
    string? LastClassName,
    long PassingAcademicYearId,
    string? PassingAcademicYearName,
    decimal? FinalPercentage,
    string CurrentStatus,
    string? Occupation,
    string? CompanyName,
    string? Industry,
    string? HigherEducationInstitution,
    string? HigherEducationCourse,
    int? HigherEducationYear,
    string? ContactEmail,
    string? ContactMobile,
    string? Address,
    string? SocialLinks,
    string? PhotoUrl,
    string PrivacySettings,
    string CommunicationPreferences,
    string? Tags,
    DateOnly? LastInteractionDate,
    bool IsDeceased,
    bool IsArchived,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record AlumniLightResponse(
    long Id,
    string AlumniId,
    string FullName,
    string? ContactMobile,
    long LastClassId,
    DateOnly GraduationDate,
    string CurrentStatus
);

// ==================== QUERY REQUESTS ====================

public record GetAlumniRequest : PagedRequest
{
    public string? Status { get; init; }
    public long? LastClassId { get; init; }
    public long? PassingAcademicYearId { get; init; }
    public string? Industry { get; init; }
    public bool? IsArchived { get; init; }
}
