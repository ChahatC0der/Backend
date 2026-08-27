namespace SchoolERP.Application.Features.Rbac.DTOs;

public record CreateRoleRequest(
    string Name,
    string Code,
    string? Description,
    bool IsBuiltin = false,
    long? BaseRoleId = null,
    List<long>? PermissionIds = null
);