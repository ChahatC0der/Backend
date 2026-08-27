namespace SchoolERP.Application.Features.Rbac.DTOs;

public record RoleAssignmentResponse(
    long UserRoleId,
    long RoleId,
    string RoleName,
    string RoleCode,
    string ScopeType,
    string? ScopeValue,
    DateTime ValidFrom,
    DateTime? ValidTo
);