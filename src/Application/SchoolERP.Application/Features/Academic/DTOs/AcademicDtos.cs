using SchoolERP.Application.Common.DTOs; // PagedRequest

namespace SchoolERP.Application.Features.Academic.DTOs;

// ==================== ACADEMIC YEAR ====================

public record CreateAcademicYearRequest(
    string Name,
    DateOnly StartDate,
    DateOnly EndDate,
    bool IsCurrent = false
);

public record UpdateAcademicYearRequest(
    long Id,
    string Name,
    DateOnly StartDate,
    DateOnly EndDate,
    bool IsCurrent,
    string Status
);

public record PatchAcademicYearRequest(
    long Id,
    string? Name = null,
    DateOnly? StartDate = null,
    DateOnly? EndDate = null,
    bool? IsCurrent = null,
    string? Status = null
);

public record BulkUpdateAcademicYearRequest(
    List<long> Ids,
    string Name,
    DateOnly StartDate,
    DateOnly EndDate,
    bool IsCurrent,
    string Status
);

public record BulkPatchAcademicYearRequest(
    List<long> Ids,
    string? Name = null,
    DateOnly? StartDate = null,
    DateOnly? EndDate = null,
    bool? IsCurrent = null,
    string? Status = null
);

public record BulkDeleteAcademicYearRequest(List<long> Ids);

public record AcademicYearResponse(
    long Id,
    Guid TenantId,
    Guid BranchId,
    string Name,
    DateOnly StartDate,
    DateOnly EndDate,
    bool IsCurrent,
    string Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record AcademicYearLightResponse(
    long Id,
    string Name,
    bool IsCurrent
);

// ==================== CLASS GROUP ====================

public record CreateClassGroupRequest(
    string Name,
    byte Sequence,
    string? Description,
    bool IsActive = true
);

public record UpdateClassGroupRequest(
    long Id,
    string Name,
    byte Sequence,
    string? Description,
    bool IsActive
);

public record PatchClassGroupRequest(
    long Id,
    string? Name = null,
    byte? Sequence = null,
    string? Description = null,
    bool? IsActive = null
);

public record BulkUpdateClassGroupRequest(
    List<long> Ids,
    string Name,
    byte Sequence,
    string? Description,
    bool IsActive
);

public record BulkPatchClassGroupRequest(
    List<long> Ids,
    string? Name = null,
    byte? Sequence = null,
    string? Description = null,
    bool? IsActive = null
);

public record BulkDeleteClassGroupRequest(List<long> Ids);

public record ClassGroupResponse(
    long Id,
    Guid TenantId,
    Guid BranchId,
    string Name,
    byte Sequence,
    string? Description,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record ClassGroupLightResponse(
    long Id,
    string Name,
    byte Sequence
);

// ==================== CLASS ====================

public record CreateClassRequest(
    string Name,
    byte Sequence,
    long? ClassGroupId = null,
    bool IsActive = true
);

public record UpdateClassRequest(
    long Id,
    string Name,
    byte Sequence,
    long? ClassGroupId,
    bool IsActive
);

public record PatchClassRequest(
    long Id,
    string? Name = null,
    byte? Sequence = null,
    long? ClassGroupId = null,
    bool? IsActive = null
);

public record BulkUpdateClassRequest(
    List<long> Ids,
    string Name,
    byte Sequence,
    long? ClassGroupId,
    bool IsActive
);

public record BulkPatchClassRequest(
    List<long> Ids,
    string? Name = null,
    byte? Sequence = null,
    long? ClassGroupId = null,
    bool? IsActive = null
);

public record BulkDeleteClassRequest(List<long> Ids);

public record ClassResponse(
    long Id,
    Guid TenantId,
    Guid BranchId,
    string Name,
    byte Sequence,
    long? ClassGroupId,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record ClassLightResponse(
    long Id,
    string Name,
    byte Sequence
);

// ==================== SECTION ====================

public record CreateSectionRequest(
    long ClassId,
    string Name,
    int? Capacity
);

public record UpdateSectionRequest(
    long Id,
    long ClassId,
    string Name,
    int? Capacity
);

public record PatchSectionRequest(
    long Id,
    long? ClassId = null,
    string? Name = null,
    int? Capacity = null
);

public record BulkUpdateSectionRequest(
    List<long> Ids,
    long ClassId,
    string Name,
    int? Capacity
);

public record BulkPatchSectionRequest(
    List<long> Ids,
    long? ClassId = null,
    string? Name = null,
    int? Capacity = null
);

public record BulkDeleteSectionRequest(List<long> Ids);

public record SectionResponse(
    long Id,
    Guid TenantId,
    Guid BranchId,
    long ClassId,
    string Name,
    int? Capacity,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record SectionLightResponse(
    long Id,
    long ClassId,
    string Name
);