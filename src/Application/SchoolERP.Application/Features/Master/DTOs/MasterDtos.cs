namespace SchoolERP.Application.Features.Master.DTOs;

// ==================== MASTER CATEGORY ====================

public record CreateMasterCategoryRequest(
    long ModuleId,
    string Key,
    string Name,
    string? Description = null,
    bool IsSystem = false,
    bool IsActive = true
);

public record UpdateMasterCategoryRequest(
    long Id,
    long ModuleId,
    string Key,
    string Name,
    string? Description,
    bool IsSystem,
    bool IsActive
);

public record PatchMasterCategoryRequest(
    long Id,
    long? ModuleId = null,
    string? Key = null,
    string? Name = null,
    string? Description = null,
    bool? IsSystem = null,
    bool? IsActive = null
);

public record BulkUpdateMasterCategoryRequest(
    List<long> Ids,
    long ModuleId,
    string Key,
    string Name,
    string? Description,
    bool IsSystem,
    bool IsActive
);

public record BulkPatchMasterCategoryRequest(
    List<long> Ids,
    long? ModuleId = null,
    string? Key = null,
    string? Name = null,
    string? Description = null,
    bool? IsSystem = null,
    bool? IsActive = null
);

public record BulkDeleteMasterCategoryRequest(List<long> Ids);

public record MasterCategoryResponse(
    long Id,
    long ModuleId,
    Guid? TenantId,
    string Key,
    string Name,
    string? Description,
    bool IsSystem,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record MasterCategoryLightResponse(
    long Id,
    string Key,
    string Name,
    bool IsActive
);

// ==================== MASTER ITEM ====================

public record CreateMasterItemRequest(
    long CategoryId,
    string Value,
    string? Code = null,
    string? Description = null,
    string? Metadata = null,
    int SortOrder = 0,
    bool IsSystem = false,
    bool IsActive = true
);

public record UpdateMasterItemRequest(
    long Id,
    long CategoryId,
    string Value,
    string? Code,
    string? Description,
    string? Metadata,
    int SortOrder,
    bool IsSystem,
    bool IsActive
);

public record PatchMasterItemRequest(
    long Id,
    long? CategoryId = null,
    string? Value = null,
    string? Code = null,
    string? Description = null,
    string? Metadata = null,
    int? SortOrder = null,
    bool? IsSystem = null,
    bool? IsActive = null
);

public record BulkUpdateMasterItemRequest(
    List<long> Ids,
    long CategoryId,
    string Value,
    string? Code,
    string? Description,
    string? Metadata,
    int SortOrder,
    bool IsSystem,
    bool IsActive
);

public record BulkPatchMasterItemRequest(
    List<long> Ids,
    long? CategoryId = null,
    string? Value = null,
    string? Code = null,
    string? Description = null,
    string? Metadata = null,
    int? SortOrder = null,
    bool? IsSystem = null,
    bool? IsActive = null
);

public record BulkDeleteMasterItemRequest(List<long> Ids);

public record MasterItemResponse(
    long Id,
    long CategoryId,
    string Value,
    string? Code,
    string? Description,
    string? Metadata,
    int SortOrder,
    bool IsSystem,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record MasterItemLightResponse(
    long Id,
    string Value,
    string? Code,
    int SortOrder
);