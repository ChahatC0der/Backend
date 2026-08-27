namespace SchoolERP.Application.Features.Rbac.DTOs;

public record UpdateRoleRequest(
    long Id,
    string Name,
    string Code,
    string? Description,
    long? BaseRoleId,
    List<long>? PermissionIds
);