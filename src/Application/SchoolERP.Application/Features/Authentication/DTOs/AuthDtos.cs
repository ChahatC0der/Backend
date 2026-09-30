using SchoolERP.Application.Features.Rbac.DTOs;

namespace SchoolERP.Application.Features.Auth.DTOs;

public record LoginRequest(string Email, string Password);
public record LoginResponse(string Token, UserResponse User);

public record GetMeResponse(
    UserContextDto User,
    List<RoleDto> Roles,
    List<string> Permissions,
    ScopesDto Scopes
);

public class UserContextDto // Class used for Dapper mapping
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public Guid TenantId { get; set; }
    public string TenantName { get; set; } = string.Empty;
    public string? TenantLogoUrl { get; set; }
    public Guid? DefaultBranchId { get; set; }
    public string? DefaultBranchName { get; set; }
    public bool IsPlatformAdmin { get; set; }
    public int PermissionsVersion { get; set; }
}

public class RoleDto { 
    public long Id { get; set; } 
    public string Name { get; set; } = string.Empty; 
    public string Code { get; set; } = string.Empty;
    public string? BaseRoleCode { get; set; } = string.Empty;
}

public class PermissionDto { public string Key { get; set; } = string.Empty; }
public class ScopeDto { public string ScopeType { get; set; } = string.Empty; public string ScopeValue { get; set; } = string.Empty; }

public record ScopesDto(
    List<string> Branches,
    List<string> Classes,
    List<string> Students,
    List<string> TransportRoutes
);