using SchoolERP.Application.Common.DTOs;

namespace SchoolERP.Application.Features.Student.DTOs;

// ==================== REGISTRATION ====================

public record CreateRegistrationRequest(
    long AcademicYearId,
    long ClassId,
    long? SectionId,
    long? EnquiryId,
    string StudentFirstName,
    string? StudentLastName,
    DateOnly DateOfBirth,
    string ParentPrimaryMobile,
    string? Gender = null,
    string? Birthplace = null,
    string? Nationality = null,
    string? MotherTongue = null,
    string? Religion = null,
    string? AadharNumber = null,
    string? FatherName = null,
    string? FatherMobile = null,
    string? FatherEmail = null,
    string? FatherOccupation = null,
    string? MotherName = null,
    string? MotherMobile = null,
    string? MotherEmail = null,
    string? MotherOccupation = null,
    string? GuardianName = null,
    string? GuardianMobile = null,
    string? GuardianEmail = null,
    string? GuardianOccupation = null,
    string? ParentPrimaryEmail = null,
    string? PresentAddress = null,
    string? PermanentAddress = null,
    bool IsPermanentSame = false,
    string? PreviousSchool = null,
    string? PreviousBoard = null,
    decimal? PreviousPercentage = null,
    int? PreviousYearPassing = null,
    string? TransferCertificateUrl = null,
    string? Source = null,
    string? MedicalConditions = null,
    string? CustomFields = null
);

public record UpdateRegistrationRequest(
    long Id,
    long AcademicYearId,
    long ClassId,
    long? SectionId,
    string StudentFirstName,
    string? StudentLastName,
    DateOnly DateOfBirth,
    string ParentPrimaryMobile,
    string? Gender,
    string? Birthplace,
    string? Nationality,
    string? MotherTongue,
    string? Religion,
    string? AadharNumber,
    string? FatherName,
    string? FatherMobile,
    string? FatherEmail,
    string? FatherOccupation,
    string? MotherName,
    string? MotherMobile,
    string? MotherEmail,
    string? MotherOccupation,
    string? GuardianName,
    string? GuardianMobile,
    string? GuardianEmail,
    string? GuardianOccupation,
    string? ParentPrimaryEmail,
    string? PresentAddress,
    string? PermanentAddress,
    bool IsPermanentSame,
    string? PreviousSchool,
    string? PreviousBoard,
    decimal? PreviousPercentage,
    int? PreviousYearPassing,
    string? TransferCertificateUrl,
    string? Source,
    string? MedicalConditions,
    string? CustomFields
);

public record PatchRegistrationRequest(
    long Id,
    long? AcademicYearId = null,
    long? ClassId = null,
    long? SectionId = null,
    string? StudentFirstName = null,
    string? StudentLastName = null,
    DateOnly? DateOfBirth = null,
    string? ParentPrimaryMobile = null,
    string? Gender = null,
    string? Birthplace = null,
    string? Nationality = null,
    string? MotherTongue = null,
    string? Religion = null,
    string? AadharNumber = null,
    string? FatherName = null,
    string? FatherMobile = null,
    string? FatherEmail = null,
    string? FatherOccupation = null,
    string? MotherName = null,
    string? MotherMobile = null,
    string? MotherEmail = null,
    string? MotherOccupation = null,
    string? GuardianName = null,
    string? GuardianMobile = null,
    string? GuardianEmail = null,
    string? GuardianOccupation = null,
    string? ParentPrimaryEmail = null,
    string? PresentAddress = null,
    string? PermanentAddress = null,
    bool? IsPermanentSame = null,
    string? PreviousSchool = null,
    string? PreviousBoard = null,
    decimal? PreviousPercentage = null,
    int? PreviousYearPassing = null,
    string? TransferCertificateUrl = null,
    string? Source = null,
    string? MedicalConditions = null,
    string? CustomFields = null
);

public record RegistrationResponse(
    long Id,
    Guid TenantId,
    Guid BranchId,
    long AcademicYearId,
    long? EnquiryId,
    string RegistrationNumber,
    string StudentFirstName,
    string? StudentLastName,
    DateOnly DateOfBirth,
    string? Gender,
    string? Birthplace,
    string? Nationality,
    string? MotherTongue,
    string? Religion,
    string? AadharNumber,
    string? FatherName,
    string? FatherMobile,
    string? FatherEmail,
    string? FatherOccupation,
    string? MotherName,
    string? MotherMobile,
    string? MotherEmail,
    string? MotherOccupation,
    string? GuardianName,
    string? GuardianMobile,
    string? GuardianEmail,
    string? GuardianOccupation,
    string ParentPrimaryMobile,
    string? ParentPrimaryEmail,
    string? PresentAddress,
    string? PermanentAddress,
    bool IsPermanentSame,
    long ClassId,
    string? ClassName,
    long? SectionId,
    string? SectionName,
    string? PreviousSchool,
    string? PreviousBoard,
    decimal? PreviousPercentage,
    int? PreviousYearPassing,
    string? TransferCertificateUrl,
    string? Source,
    string? MedicalConditions,
    string? CustomFields,
    bool RegistrationFeePaid,
    decimal? RegistrationFeeAmount,
    DateOnly? RegistrationFeePaymentDate,
    string? RegistrationFeePaymentMode,
    DateTime? InterviewDate,
    string? InterviewMode,
    long? InterviewerId,
    string? InterviewerName,
    string? InterviewResult,
    string? InterviewFeedback,
    DateTime? ApprovedAt,
    long? ApprovedBy,
    string? ApprovedByName,
    string? RejectionReason,
    long? ParentUserId,
    long? ConvertedToStudentId,
    string Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record RegistrationLightResponse(
    long Id,
    string RegistrationNumber,
    string StudentFullName,
    string ParentPrimaryMobile,
    long ClassId,
    string Status,
    DateTime CreatedAt
);

// ==================== WORKFLOW REQUESTS ====================

public record SubmitRegistrationRequest(long Id);

public record StartRegistrationReviewRequest(long Id);

public record ScheduleInterviewRequest(
    long Id,
    DateTime InterviewDate,
    string InterviewMode,
    long InterviewerId
);

public record RecordInterviewResultRequest(
    long Id,
    string InterviewResult,
    string? InterviewFeedback = null
);

public record ApproveRegistrationRequest(long Id);

public record RejectRegistrationRequest(
    long Id,
    string RejectionReason
);

public record ConvertRegistrationToAdmissionRequest(
    long Id,
    long AcademicYearId,
    long ClassId,
    long SectionId
);

public record RecordRegistrationFeeRequest(
    long Id,
    decimal Amount,
    string PaymentMode,
    DateOnly PaymentDate
);

// ==================== REGISTRATION DOCUMENT ====================

public record CreateRegistrationDocumentRequest(
    long RegistrationId,
    string DocumentType,
    string? FileUrl = null,
    string? Description = null
);

public record UpdateRegistrationDocumentRequest(
    long Id,
    string DocumentType,
    string? FileUrl,
    string? Description,
    string Status,
    string? RejectionReason
);

public record PatchRegistrationDocumentRequest(
    long Id,
    string? DocumentType = null,
    string? FileUrl = null,
    string? Description = null,
    string? Status = null,
    string? RejectionReason = null
);

public record VerifyRegistrationDocumentRequest(
    long Id,
    bool Verified,
    string? RejectionReason = null
);

public record RegistrationDocumentResponse(
    long Id,
    long RegistrationId,
    string DocumentType,
    string? FileUrl,
    string? Description,
    string Status,
    string? RejectionReason,
    long? VerifiedBy,
    DateTime? VerifiedAt,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record RegistrationDocumentLightResponse(
    long Id,
    string DocumentType,
    string Status,
    string? FileUrl
);

// ==================== QUERY REQUESTS ====================

public record GetRegistrationsRequest : PagedRequest
{
    public string? Status { get; init; }
    public long? ClassId { get; init; }
    public long? AcademicYearId { get; init; }
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
}
