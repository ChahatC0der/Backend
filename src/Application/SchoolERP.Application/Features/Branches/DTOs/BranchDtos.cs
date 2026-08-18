namespace SchoolERP.Application.Features.Branches.DTOs;

public record CreateBranchRequest(
    string Name,
    string Code,
    string? Address = null,
    string? Phone = null,
    string? Email = null,
    string? ContactPerson = null,
    bool IsDefault = false
);

public record UpdateBranchRequest(
    string Name,
    string Code,
    string? Address = null,
    string? Phone = null,
    string? Email = null,
    string? ContactPerson = null,
    bool IsDefault = false,
    string Status = "active"
);

public record PatchBranchRequest(
    string? Name = null,
    string? Code = null,
    string? Address = null,
    string? Phone = null,
    string? Email = null,
    string? ContactPerson = null,
    bool? IsDefault = null,
    string? Status = null
);

public record BranchResponse(
    Guid Id,
    Guid TenantId,
    string Name,
    string Code,
    string? Address,
    string? Phone,
    string? Email,
    string? ContactPerson,
    bool IsDefault,
    string Status,
    DateTime CreatedAt
);

public record BranchLightResponse(
    Guid Id,
    string Name,
    string Code
);

public record BulkDeleteBranchRequest(List<Guid> Ids);

public record BulkUpdateBranchRequest(
    List<Guid> Ids,
    string? Status = null,
    bool? IsDefault = null
);