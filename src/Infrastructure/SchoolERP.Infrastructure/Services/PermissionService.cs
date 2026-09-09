using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Domain.Rbac.Entities;

public class PermissionService : IPermissionService
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentBranchService _branchService;
    private readonly IMemoryCache _cache;
    private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(5);

    public PermissionService(
        IApplicationDbContext dbContext,
        ICurrentBranchService branchService,
        IMemoryCache cache)
    {
        _dbContext = dbContext;
        _branchService = branchService;
        _cache = cache;
    }

    public async Task<bool> HasPermissionAsync(long userId, string permissionKey, CancellationToken cancellationToken = default)
    {
        // Platform admin bypass
        var user = await _dbContext.Set<User>()
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted, cancellationToken);
        if (user?.IsPlatformAdmin == true) return true;
        if (user == null) return false;

        var currentBranchId = _branchService.GetBranchId();
        string cacheKey = $"user_permissions_{userId}_{user.PermissionsVersion}_{currentBranchId?.ToString() ?? "all"}";
        if (!_cache.TryGetValue(cacheKey, out HashSet<string>? permissions))
        {
            permissions = await LoadPermissionsFromDb(userId, currentBranchId, cancellationToken);
            _cache.Set(cacheKey, permissions, _cacheDuration);
        }

        return permissions!.Contains(permissionKey) || permissions.Contains("*.*");
    }

    private async Task<HashSet<string>> LoadPermissionsFromDb(long userId, Guid? branchId, CancellationToken ct)
    {
        var result = new HashSet<string>();

        // Base query for user roles with role and permissions
        var query = _dbContext.Set<UserRole>()
            .AsNoTracking()
            .Include(ur => ur.Role)
                .ThenInclude(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
            .Where(ur => ur.UserId == userId &&
                         ur.ValidFrom <= DateTime.UtcNow.Date &&
                         (ur.ValidTo == null || ur.ValidTo >= DateTime.UtcNow.Date));

        // Apply branch filter if branchId is provided
        if (branchId.HasValue)
        {
            var branchIdString = branchId.Value.ToString();
            query = query.Where(ur =>
                ur.ScopeType == "tenant" ||                           // tenant-wide roles
                (ur.ScopeType == "branch" && ur.ScopeValue == branchIdString)); // roles for this branch
        }
        // If no branch selected, we may skip branch-scoped roles entirely? or include only tenant? decide business rule.
        else
        {
            // If no branch context, only include tenant-wide roles (or maybe none)
            query = query.Where(ur => ur.ScopeType == "tenant");
        }

        var assignments = await query.ToListAsync(ct);

        foreach (var assignment in assignments)
        {
            var role = assignment.Role;
            foreach (var rp in role.RolePermissions)
                result.Add(rp.Permission.Key);

            // Handle inheritance
            var baseRoleId = role.BaseRoleId;
            while (baseRoleId.HasValue)
            {
                var baseRole = await _dbContext.Set<Role>()
                    .AsNoTracking()
                    .Include(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
                    .FirstOrDefaultAsync(r => r.Id == baseRoleId.Value, ct);
                if (baseRole == null) break;
                foreach (var rp in baseRole.RolePermissions)
                    result.Add(rp.Permission.Key);
                baseRoleId = baseRole.BaseRoleId;
            }
        }

        return result;
    }
}