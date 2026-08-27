namespace SchoolERP.Application.Features.Rbac.DTOs;

public record AssignRoleRequest(
    long UserId,
    long RoleId,
    string ScopeType,
    string? ScopeValue,
    DateTime? ValidFrom = null,
    DateTime? ValidTo = null
);