namespace SchoolERP.Application.Features.Rbac.DTOs;

public record PermissionResponse(
    long Id,
    string Key,
    string Action,
    string ModuleKey,
    string? Description
);