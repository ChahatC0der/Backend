namespace SchoolERP.Application.Features.Branches.DTOs;

// 🔥 CREATE
public record CreateBranchRequest(
    string Name,
    string Code,
    string Email,               // 👈 NON-NULLABLE
    bool IsDefault = false,
    string? Address = null,
    string? Phone = null,
    string? ContactPerson = null
);

// 🔥 UPDATE (FULL)
public record UpdateBranchRequest(
    string Name,
    string Code,
    string Email,               // 👈 NON-NULLABLE
    bool IsDefault = false,
    string Status = "active",
    string? Address = null,
    string? Phone = null,
    string? ContactPerson = null
);

// 🔥 PATCH (PARTIAL)
public record PatchBranchRequest(
    string? Name = null,
    string? Code = null,
    string? Address = null,
    string? Phone = null,
    string? Email = null,       // 👈 NULLABLE (patch me optional)
    string? ContactPerson = null,
    bool? IsDefault = null,
    string? Status = null
);

// 🔥 RESPONSE
public record BranchResponse(
    Guid Id,
    Guid TenantId,
    string Name,
    string Code,
    string? Address,
    string? Phone,
    string Email,
    string? ContactPerson,
    bool IsDefault,
    string Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

// 🔥 LIGHTWEIGHT (Dropdowns)
public record BranchLightResponse(
    Guid Id,
    string Name,
    string Code
);

// 🔥 BULK DELETE
public record BulkDeleteBranchRequest(List<Guid> Ids);

// 🔥 BULK UPDATE
public record BulkUpdateBranchRequest(
    List<Guid> Ids,
    string? Status = null,
    bool? IsDefault = null
);

// 🔥 BULK PATCH
public record BulkPatchBranchRequest(
    List<Guid> Ids,
    string? Name = null,
    string? Code = null,
    string? Address = null,
    string? Phone = null,
    string? Email = null,
    string? ContactPerson = null,
    bool? IsDefault = null,
    string? Status = null
);