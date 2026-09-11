namespace SchoolERP.Application.Features.Staff.DTOs;

// ==================== DEPARTMENT ====================

public record CreateDepartmentRequest(
    string Name,
    string Code,
    long? HodId = null
);

public record UpdateDepartmentRequest(
    long Id,
    string Name,
    string Code,
    long? HodId
);

public record PatchDepartmentRequest(
    long Id,
    string? Name = null,
    string? Code = null,
    long? HodId = null
);

public record BulkUpdateDepartmentRequest(
    List<long> Ids,
    string Name,
    string Code,
    long? HodId
);

public record BulkPatchDepartmentRequest(
    List<long> Ids,
    string? Name = null,
    string? Code = null,
    long? HodId = null
);

public record BulkDeleteDepartmentRequest(List<long> Ids);

public record DepartmentResponse(
    long Id,
    Guid BranchId,
    string Name,
    string Code,
    long? HodId,
    string? HodName,
    int StaffCount,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record DepartmentLightResponse(
    long Id,
    string Name,
    string Code
);

// ==================== STAFF ====================

public record CreateStaffRequest(
    // Professional
    DateOnly JoiningDate,
    string EmploymentType,         // permanent, contract, probation, intern
    string StaffType,              // teaching, non_teaching
    string Designation,
    long? DepartmentId = null,
    long? UserId = null,

    // Personal
    string FirstName = "",
    string? LastName = null,
    DateOnly? DateOfBirth = null,
    string? Gender = null,
    string? BloodGroup = null,
    string? MaritalStatus = null,
    string? Nationality = null,

    // Contact
    string? PersonalEmail = null,
    string? WorkEmail = null,
    string? PersonalMobile = null,
    string? WorkMobile = null,
    string? EmergencyContactName = null,
    string? EmergencyContactPhone = null,

    // Address
    string? PresentAddress = null,
    string? PermanentAddress = null,

    // Academic
    string? Qualification = null,
    string? Specialization = null,
    decimal? ExperienceYears = null,
    string? PreviousInstitution = null,

    string? PhotoUrl = null,
    string? CustomFields = null    // JSON
);

public record UpdateStaffRequest(
    long Id,
    DateOnly JoiningDate,
    string EmploymentType,
    string StaffType,
    string Designation,
    long? DepartmentId,
    long? UserId,

    string FirstName,
    string? LastName,
    DateOnly? DateOfBirth,
    string? Gender,
    string? BloodGroup,
    string? MaritalStatus,
    string? Nationality,

    string? PersonalEmail,
    string? WorkEmail,
    string? PersonalMobile,
    string? WorkMobile,
    string? EmergencyContactName,
    string? EmergencyContactPhone,

    string? PresentAddress,
    string? PermanentAddress,

    string? Qualification,
    string? Specialization,
    decimal? ExperienceYears,
    string? PreviousInstitution,

    string? PhotoUrl,
    string? Status,
    string? CustomFields
);

public record PatchStaffRequest(
    long Id,
    DateOnly? JoiningDate = null,
    string? EmploymentType = null,
    string? StaffType = null,
    string? Designation = null,
    long? DepartmentId = null,
    long? UserId = null,

    string? FirstName = null,
    string? LastName = null,
    DateOnly? DateOfBirth = null,
    string? Gender = null,
    string? BloodGroup = null,
    string? MaritalStatus = null,
    string? Nationality = null,

    string? PersonalEmail = null,
    string? WorkEmail = null,
    string? PersonalMobile = null,
    string? WorkMobile = null,
    string? EmergencyContactName = null,
    string? EmergencyContactPhone = null,

    string? PresentAddress = null,
    string? PermanentAddress = null,

    string? Qualification = null,
    string? Specialization = null,
    decimal? ExperienceYears = null,
    string? PreviousInstitution = null,

    string? PhotoUrl = null,
    string? Status = null,
    string? CustomFields = null
);

public record BulkUpdateStaffRequest(
    List<long> Ids,
    string EmploymentType,
    string StaffType,
    string Designation,
    long? DepartmentId,
    string? Status
);

public record BulkPatchStaffRequest(
    List<long> Ids,
    string? EmploymentType = null,
    string? StaffType = null,
    string? Designation = null,
    long? DepartmentId = null,
    string? Status = null
);

public record BulkDeleteStaffRequest(List<long> Ids);

public record StaffResponse(
    long Id,
    Guid TenantId,
    Guid BranchId,
    string StaffId,
    long? UserId,
    long? DepartmentId,
    string? DepartmentName,

    string FirstName,
    string? LastName,
    DateOnly? DateOfBirth,
    string? Gender,
    string? BloodGroup,
    string? MaritalStatus,
    string? Nationality,

    string? PersonalEmail,
    string? WorkEmail,
    string? PersonalMobile,
    string? WorkMobile,
    string? EmergencyContactName,
    string? EmergencyContactPhone,

    string? PresentAddress,
    string? PermanentAddress,

    DateOnly JoiningDate,
    string EmploymentType,
    string StaffType,
    string Designation,

    string? Qualification,
    string? Specialization,
    decimal? ExperienceYears,
    string? PreviousInstitution,

    string? PhotoUrl,
    string Status,
    string? CustomFields,

    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record StaffLightResponse(
    long Id,
    string StaffId,
    string FullName,
    string Designation,
    string StaffType,
    string Status
);

// ==================== STAFF DOCUMENT ====================

public record CreateStaffDocumentRequest(
    long StaffId,
    string DocumentType,
    string FileUrl,
    DateOnly? ExpiryDate = null
);

public record UpdateStaffDocumentRequest(
    long Id,
    long StaffId,
    string DocumentType,
    string FileUrl,
    DateOnly? ExpiryDate,
    string VerificationStatus,
    long? VerifiedBy
);

public record PatchStaffDocumentRequest(
    long Id,
    string? DocumentType = null,
    string? FileUrl = null,
    DateOnly? ExpiryDate = null,
    string? VerificationStatus = null,
    long? VerifiedBy = null
);

public record BulkUpdateStaffDocumentRequest(
    List<long> Ids,
    string DocumentType,
    string VerificationStatus
);

public record BulkPatchStaffDocumentRequest(
    List<long> Ids,
    string? DocumentType = null,
    string? VerificationStatus = null
);

public record BulkDeleteStaffDocumentRequest(List<long> Ids);

public record StaffDocumentResponse(
    long Id,
    long StaffId,
    string DocumentType,
    string FileUrl,
    DateOnly? ExpiryDate,
    string VerificationStatus,
    long? VerifiedBy,
    DateTime? VerifiedAt,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record StaffDocumentLightResponse(
    long Id,
    string DocumentType,
    string FileUrl,
    DateOnly? ExpiryDate,
    string VerificationStatus
);