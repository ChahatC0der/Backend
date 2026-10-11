namespace SchoolERP.Application.Features.Rbac.DTOs;

public record CreateModuleRequest(
    long? ParentId,
    string Name,
    string Key,
    string? Description = null,
    int SortOrder = 0
);

public record UpdateModuleRequest(
    long Id,
    long? ParentId,
    string Name,
    string Key,
    string? Description,
    int SortOrder
);

public record PatchModuleRequest(
    long Id,
    long? ParentId = null,
    string? Name = null,
    string? Key = null,
    string? Description = null,
    int? SortOrder = null
);

public record ModuleResponse(
    long Id,
    long? ParentId,
    Guid? TenantId,
    string Name,
    string Key,
    string? Description,
    bool IsSystem,
    int SortOrder,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record ModuleLightResponse(
    long Id,
    string Name,
    string Key,
    int SortOrder
);