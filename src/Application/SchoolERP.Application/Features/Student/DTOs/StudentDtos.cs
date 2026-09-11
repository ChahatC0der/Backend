namespace SchoolERP.Application.Features.Student.DTOs;

// ==================== STUDENT ====================

public record CreateStudentRequest(
    string FirstName,
    string? LastName,
    DateOnly DateOfBirth,
    DateOnly AdmissionDate,
    string? Gender = null,
    string? BloodGroup = null,
    string? Nationality = null,
    string? Religion = null,
    string? MotherTongue = null,
    string? PersonalEmail = null,
    string? PersonalMobile = null,
    string? AlternateMobile = null,
    string? Address = null,
    string? RollNumber = null,
    string Status = "enrolled",
    string? PhotoUrl = null,
    string? CustomFields = null,
    long? UserId = null
);

public record UpdateStudentRequest(
    long Id,
    string FirstName,
    string? LastName,
    DateOnly DateOfBirth,
    DateOnly AdmissionDate,
    string? Gender,
    string? BloodGroup,
    string? Nationality,
    string? Religion,
    string? MotherTongue,
    string? PersonalEmail,
    string? PersonalMobile,
    string? AlternateMobile,
    string? Address,
    string? RollNumber,
    string Status,
    string? PhotoUrl,
    string? CustomFields,
    long? UserId
);

public record PatchStudentRequest(
    long Id,
    string? FirstName = null,
    string? LastName = null,
    DateOnly? DateOfBirth = null,
    DateOnly? AdmissionDate = null,
    string? Gender = null,
    string? BloodGroup = null,
    string? Nationality = null,
    string? Religion = null,
    string? MotherTongue = null,
    string? PersonalEmail = null,
    string? PersonalMobile = null,
    string? AlternateMobile = null,
    string? Address = null,
    string? RollNumber = null,
    string? Status = null,
    string? PhotoUrl = null,
    string? CustomFields = null,
    long? UserId = null
);

public record BulkUpdateStudentRequest(
    List<long> Ids,
    string Status,
    string? BloodGroup,
    string? Nationality
);

public record BulkPatchStudentRequest(
    List<long> Ids,
    string? Status = null,
    string? BloodGroup = null,
    string? Nationality = null
);

public record BulkDeleteStudentRequest(List<long> Ids);

public record StudentResponse(
    long Id,
    Guid TenantId,
    Guid BranchId,
    string EnrollmentId,
    long? UserId,
    string FirstName,
    string? LastName,
    DateOnly DateOfBirth,
    string? Gender,
    string? BloodGroup,
    string? Nationality,
    string? Religion,
    string? MotherTongue,
    string? PersonalEmail,
    string? PersonalMobile,
    string? AlternateMobile,
    string? Address,
    DateOnly AdmissionDate,
    string? RollNumber,
    string Status,
    string? PhotoUrl,
    string? CustomFields,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record StudentLightResponse(
    long Id,
    string EnrollmentId,
    string FullName,
    string? RollNumber,
    string Status
);

// ==================== STUDENT ENROLLMENT ====================

public record CreateStudentEnrollmentRequest(
    long StudentId,
    long AcademicYearId,
    long ClassId,
    long SectionId,
    string? RollNumber = null,
    DateOnly? EnrolledAt = null
);

public record UpdateStudentEnrollmentRequest(
    long Id,
    long AcademicYearId,
    long ClassId,
    long SectionId,
    string? RollNumber,
    bool IsCurrent,
    DateOnly EnrolledAt,
    DateOnly? LeftAt
);

public record PatchStudentEnrollmentRequest(
    long Id,
    long? AcademicYearId = null,
    long? ClassId = null,
    long? SectionId = null,
    string? RollNumber = null,
    bool? IsCurrent = null,
    DateOnly? EnrolledAt = null,
    DateOnly? LeftAt = null
);

public record BulkUpdateStudentEnrollmentRequest(
    List<long> Ids,
    long AcademicYearId,
    long ClassId,
    long SectionId,
    string? RollNumber
);

public record BulkPatchStudentEnrollmentRequest(
    List<long> Ids,
    long? AcademicYearId = null,
    long? ClassId = null,
    long? SectionId = null,
    string? RollNumber = null,
    bool? IsCurrent = null
);

public record BulkDeleteStudentEnrollmentRequest(List<long> Ids);

public record StudentEnrollmentResponse(
    long Id,
    long StudentId,
    string StudentName,
    long AcademicYearId,
    string? AcademicYearName,
    long ClassId,
    string? ClassName,
    long SectionId,
    string? SectionName,
    string? RollNumber,
    bool IsCurrent,
    DateOnly EnrolledAt,
    DateOnly? LeftAt,
    DateTime CreatedAt
);

public record StudentEnrollmentLightResponse(
    long Id,
    long StudentId,
    long AcademicYearId,
    long ClassId,
    long SectionId,
    string? RollNumber,
    bool IsCurrent
);

// ==================== PARENT ====================

public record CreateParentRequest(
    long StudentId,
    string ParentType,
    string FirstName,
    string? LastName = null,
    string? Email = null,
    string? Mobile = null,
    string? Occupation = null,
    bool IsPrimary = false,
    long? UserId = null
);

public record UpdateParentRequest(
    long Id,
    long StudentId,
    string ParentType,
    string FirstName,
    string? LastName,
    string? Email,
    string? Mobile,
    string? Occupation,
    bool IsPrimary,
    long? UserId
);

public record PatchParentRequest(
    long Id,
    string? ParentType = null,
    string? FirstName = null,
    string? LastName = null,
    string? Email = null,
    string? Mobile = null,
    string? Occupation = null,
    bool? IsPrimary = null,
    long? UserId = null
);

public record BulkUpdateParentRequest(
    List<long> Ids,
    string? Occupation,
    bool IsPrimary
);

public record BulkPatchParentRequest(
    List<long> Ids,
    string? Occupation = null,
    bool? IsPrimary = null
);

public record BulkDeleteParentRequest(List<long> Ids);

public record ParentResponse(
    long Id,
    Guid TenantId,
    long StudentId,
    string ParentType,
    string FirstName,
    string? LastName,
    string? Email,
    string? Mobile,
    string? Occupation,
    bool IsPrimary,
    long? UserId,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record ParentLightResponse(
    long Id,
    string ParentType,
    string FullName,
    string? Mobile
);

// ==================== STUDENT DOCUMENT ====================

public record CreateStudentDocumentRequest(
    long StudentId,
    string DocumentType,
    string FileUrl,
    string? Description = null
);

public record UpdateStudentDocumentRequest(
    long Id,
    long StudentId,
    string DocumentType,
    string FileUrl,
    string? Description,
    bool Verified,
    long? UploadedBy
);

public record PatchStudentDocumentRequest(
    long Id,
    string? DocumentType = null,
    string? FileUrl = null,
    string? Description = null,
    bool? Verified = null,
    long? UploadedBy = null
);

public record BulkUpdateStudentDocumentRequest(
    List<long> Ids,
    bool Verified
);

public record BulkPatchStudentDocumentRequest(
    List<long> Ids,
    bool? Verified = null
);

public record BulkDeleteStudentDocumentRequest(List<long> Ids);

public record StudentDocumentResponse(
    long Id,
    long StudentId,
    string DocumentType,
    string FileUrl,
    string? Description,
    bool Verified,
    long? UploadedBy,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record StudentDocumentLightResponse(
    long Id,
    string DocumentType,
    string FileUrl,
    bool Verified
);