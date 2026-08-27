namespace SchoolERP.Application.Features.Rbac.DTOs;

public record UserPermissionsResponse(
    long UserId,
    Guid TenantId,
    Guid? BranchId,
    int PermissionsVersion,
    List<RoleAssignmentResponse> Roles,
    List<string> Permissions
);