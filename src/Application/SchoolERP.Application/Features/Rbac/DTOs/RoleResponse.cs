namespace SchoolERP.Application.Features.Rbac.DTOs;

public record RoleResponse(
    long Id,
    Guid TenantId,
    string Name,
    string Code,
    string? Description,
    bool IsBuiltin,
    bool IsSystem,
    long? BaseRoleId,
    List<PermissionResponse> Permissions,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);