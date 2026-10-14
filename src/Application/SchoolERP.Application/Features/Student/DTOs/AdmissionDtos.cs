using SchoolERP.Application.Common.DTOs;

namespace SchoolERP.Application.Features.Student.DTOs;

// ==================== ADMISSION ====================

public record CreateAdmissionRequest(
    long AcademicYearId,
    long ClassId,
    long SectionId,
    long? RegistrationId = null,
    long? EnquiryId = null,
    DateOnly? AdmissionDate = null,
    string? AdmissionNumber = null,
    string? RollNumber = null,
    DateOnly? JoiningDate = null,
    string? PreviousSchool = null,
    long? TransportRouteId = null
);

public record UpdateAdmissionRequest(
    long Id,
    long AcademicYearId,
    long ClassId,
    long SectionId,
    DateOnly AdmissionDate,
    string? AdmissionNumber,
    string? RollNumber,
    DateOnly? JoiningDate,
    string? PreviousSchool,
    long? TransportRouteId
);

public record PatchAdmissionRequest(
    long Id,
    long? AcademicYearId = null,
    long? ClassId = null,
    long? SectionId = null,
    DateOnly? AdmissionDate = null,
    string? AdmissionNumber = null,
    string? RollNumber = null,
    DateOnly? JoiningDate = null,
    string? PreviousSchool = null,
    long? TransportRouteId = null
);

public record AdmissionResponse(
    long Id,
    Guid TenantId,
    Guid BranchId,
    long AcademicYearId,
    string? AcademicYearName,
    long? RegistrationId,
    string? RegistrationNumber,
    long? EnquiryId,
    long? StudentId,
    string? StudentName,
    string? EnrollmentId,
    long ClassId,
    string? ClassName,
    long SectionId,
    string? SectionName,
    DateOnly AdmissionDate,
    string? AdmissionNumber,
    string? RollNumber,
    DateOnly? JoiningDate,
    string? PreviousSchool,
    bool FirstFeePaid,
    decimal? FirstFeeAmount,
    DateOnly? FirstFeePaymentDate,
    string? FirstFeePaymentMode,
    long? TransportRouteId,
    DateTime? EnrolledAt,
    long? EnrolledBy,
    string? EnrolledByName,
    string Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record AdmissionLightResponse(
    long Id,
    string? StudentName,
    string? EnrollmentId,
    string? RegistrationNumber,
    long ClassId,
    long SectionId,
    string Status,
    DateOnly AdmissionDate
);

// ==================== WORKFLOW REQUESTS ====================

public record CollectAdmissionFeeRequest(
    long Id,
    decimal Amount,
    string PaymentMode,
    DateOnly PaymentDate
);

public record WaiveAdmissionFeeRequest(
    long Id,
    string Reason
);

public record CancelAdmissionRequest(
    long Id,
    string Reason
);

// ==================== ENROLL NOW ====================

public record EnrollNowRequest(
    long Id,
    string? RollNumber = null,
    DateOnly? JoiningDate = null,
    long? TransportRouteId = null,
    // Additional fields for Student creation if not from Registration
    string? StudentFirstName = null,
    string? StudentLastName = null,
    DateOnly? DateOfBirth = null,
    string? Gender = null,
    string? PersonalMobile = null,
    string? PersonalEmail = null,
    string? Address = null,
    // Parent info
    string ParentPrimaryMobile = "",
    string? ParentPrimaryEmail = null,
    string? FatherName = null,
    string? FatherMobile = null,
    string? MotherName = null,
    string? MotherMobile = null,
    string? GuardianName = null,
    string? GuardianMobile = null,
    // Family (optional link to existing)
    long? FamilyId = null
);

// ==================== QUERY REQUESTS ====================

public record GetAdmissionsRequest : PagedRequest
{
    public string? Status { get; init; }
    public long? ClassId { get; init; }
    public long? SectionId { get; init; }
    public long? AcademicYearId { get; init; }
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
}
