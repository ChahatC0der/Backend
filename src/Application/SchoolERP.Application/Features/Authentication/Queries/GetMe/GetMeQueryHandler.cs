using MediatR;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Auth.DTOs;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Auth.Queries.GetMe;

public class GetMeQueryHandler : IRequestHandler<GetMeQuery, Result<GetMeResponse>>
{
    private readonly IDapperRepository<UserContextDto> _userRepo;
    private readonly IDapperRepository<RoleDto> _roleRepo;
    private readonly IDapperRepository<PermissionDto> _permRepo;
    private readonly IDapperRepository<ScopeDto> _scopeRepo;

    public GetMeQueryHandler(
        IDapperRepository<UserContextDto> userRepo,
        IDapperRepository<RoleDto> roleRepo,
        IDapperRepository<PermissionDto> permRepo,
        IDapperRepository<ScopeDto> scopeRepo)
    {
        _userRepo = userRepo;
        _roleRepo = roleRepo;
        _permRepo = permRepo;
        _scopeRepo = scopeRepo;
    }

    public async Task<Result<GetMeResponse>> Handle(GetMeQuery request, CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow.Date;
        var parameters = new { UserId = request.UserId, Today = today };

        // ──────────────────────────────────────────────
        // 1. Fetch User + Tenant + Branch Context
        // ──────────────────────────────────────────────
        const string userSql = @"
            SELECT 
                u.Id, 
                u.Name, 
                u.Email, 
                u.TenantId, 
                t.Name       AS TenantName, 
                t.LogoUrl    AS TenantLogoUrl, 
                u.BranchId   AS DefaultBranchId, 
                b.Name       AS DefaultBranchName, 
                u.IsPlatformAdmin, 
                u.PermissionsVersion
            FROM dbo.Users u
            INNER JOIN dbo.Tenants t ON u.TenantId = t.Id
            LEFT  JOIN dbo.Branches b ON u.BranchId = b.Id
            WHERE u.Id = @UserId 
              AND u.IsDeleted = 0 
              AND u.Status = 'active';";

        var users = await _userRepo.ExecuteRawSqlAsync(userSql, parameters, cancellationToken);
        var user = users.FirstOrDefault();

        if (user is null)
            return Result.Failure<GetMeResponse>(Error.Unauthorized("User not found, deleted, or suspended."));

        // ──────────────────────────────────────────────
        // 2. Fetch Active Roles (union of all assignments)
        // ──────────────────────────────────────────────
        const string rolesSql = @"
            SELECT DISTINCT 
        r.Id, 
        r.Name, 
        r.Code, 
        br.Code AS BaseRoleCode  -- 🔥 MAGIC: Base role ka code directly aa jayega
    FROM dbo.UserRoles ur
    INNER JOIN dbo.Roles r ON ur.RoleId = r.Id
    LEFT JOIN dbo.Roles br ON r.BaseRoleId = br.Id  -- 🔥 Self Join
    WHERE ur.UserId = @UserId 
      AND ur.ValidFrom <= @Today 
      AND (ur.ValidTo IS NULL OR ur.ValidTo >= @Today)
      AND r.IsDeleted = 0;";

        var roles = (await _roleRepo.ExecuteRawSqlAsync(rolesSql, parameters, cancellationToken)).ToList();

        // ──────────────────────────────────────────────
        // 3. Fetch Flattened Permissions (across all roles)
        // ──────────────────────────────────────────────
        const string permSql = @"
            SELECT DISTINCT p.[Key]
            FROM dbo.UserRoles ur
            INNER JOIN dbo.RolePermissions rp ON ur.RoleId = rp.RoleId
            INNER JOIN dbo.Permissions p ON rp.PermissionId = p.Id
            WHERE ur.UserId = @UserId
              AND ur.ValidFrom <= @Today
              AND (ur.ValidTo IS NULL OR ur.ValidTo >= @Today)
              AND rp.IsDeleted = 0;";

        var perms = (await _permRepo.ExecuteRawSqlAsync(permSql, parameters, cancellationToken)).ToList();

        // ──────────────────────────────────────────────
        // 4. Fetch Scopes (where can this user act?)
        // ──────────────────────────────────────────────
        const string scopeSql = @"
            SELECT ScopeType, ScopeValue
            FROM dbo.UserRoles
            WHERE UserId = @UserId
              AND ValidFrom <= @Today
              AND (ValidTo IS NULL OR ValidTo >= @Today);";

        var scopeRows = (await _scopeRepo.ExecuteRawSqlAsync(scopeSql, parameters, cancellationToken)).ToList();

        // ──────────────────────────────────────────────
        // 5. Business Logic: Group scopes by type
        // ──────────────────────────────────────────────
        var scopes = new ScopesDto(
            Branches: [],
            Classes: [],
            Students: [],
            TransportRoutes: []
        );

        foreach (var row in scopeRows)
        {
            switch (row.ScopeType.ToLowerInvariant())
            {
                case "branch":
                    scopes.Branches.Add(row.ScopeValue);
                    break;
                case "class":
                    scopes.Classes.Add(row.ScopeValue);
                    break;
                case "student":
                    scopes.Students.Add(row.ScopeValue);
                    break;
                case "transport_route":
                    scopes.TransportRoutes.Add(row.ScopeValue);
                    break;
            }
        }

        // ──────────────────────────────────────────────
        // 6. Business Logic: Super Admin wildcard
        // ──────────────────────────────────────────────
        var permissionKeys = perms.Select(p => p.Key).ToList();

        if (user.IsPlatformAdmin && !permissionKeys.Contains("*.*"))
        {
            permissionKeys.Add("*.*");
        }

        // ──────────────────────────────────────────────
        // 7. Build & Return Response
        // ──────────────────────────────────────────────
        var response = new GetMeResponse(
            User: user,
            Roles: roles,
            Permissions: permissionKeys,
            Scopes: scopes
        );

        return Result.Success(response);
    }
}