using SchoolERP.Application.Common.DTOs;

namespace SchoolERP.Application.Features.Student.DTOs;

// ==================== FAMILY ====================

public record CreateFamilyRequest(
    string? HeadName = null,
    string? HeadMobile = null,
    string? HeadEmail = null
);

public record UpdateFamilyRequest(
    long Id,
    string? HeadName,
    string? HeadMobile,
    string? HeadEmail
);

public record PatchFamilyRequest(
    long Id,
    string? HeadName = null,
    string? HeadMobile = null,
    string? HeadEmail = null
);

public record FamilyResponse(
    long Id,
    Guid TenantId,
    Guid BranchId,
    string? HeadName,
    string? HeadMobile,
    string? HeadEmail,
    int StudentCount,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record FamilyDetailedResponse(
    long Id,
    Guid TenantId,
    Guid BranchId,
    string? HeadName,
    string? HeadMobile,
    string? HeadEmail,
    int StudentCount,
    List<FamilyStudentLightResponse> Students,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record FamilyStudentLightResponse(
    long Id,
    string EnrollmentId,
    string FullName,
    string? ClassName,
    string? SectionName,
    string Status
);

public record FamilyLightResponse(
    long Id,
    string? HeadName,
    string? HeadMobile,
    int StudentCount
);

// ==================== WORKFLOW REQUESTS ====================

public record LinkStudentToFamilyRequest(
    long FamilyId,
    long StudentId
);

public record UnlinkStudentFromFamilyRequest(
    long FamilyId,
    long StudentId
);

// ==================== QUERY REQUESTS ====================

public record GetFamiliesRequest : PagedRequest
{
    public string? HeadMobile { get; init; }
}
