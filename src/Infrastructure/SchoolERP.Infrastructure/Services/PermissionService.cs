using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Domain.Rbac.Entities;

namespace SchoolERP.Infrastructure.Services;

public class PermissionService : IPermissionService
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IMemoryCache _cache;
    private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(5);

    public PermissionService(IApplicationDbContext dbContext, IMemoryCache cache)
    {
        _dbContext = dbContext;
        _cache = cache;
    }

    public async Task<bool> HasPermissionAsync(long userId, string permissionKey, CancellationToken cancellationToken)
    {
        // 1. Check platform admin (bypass)
        var user = await _dbContext.Set<User>()
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted, cancellationToken);

        if (user?.IsPlatformAdmin == true)
            return true;

        if (user == null)
            return false;

        // 2. Get permissions from cache or DB
        var cacheKey = $"user_permissions_{userId}_{user.PermissionsVersion}";
        if (!_cache.TryGetValue(cacheKey, out HashSet<string>? permissions))
        {
            permissions = await LoadPermissionsFromDb(userId, cancellationToken);
            _cache.Set(cacheKey, permissions, _cacheDuration);
        }

        return permissions!.Contains(permissionKey) || permissions.Contains("*.*");
    }

    private async Task<HashSet<string>> LoadPermissionsFromDb(long userId, CancellationToken cancellationToken)
    {
        var permissionSet = new HashSet<string>();

        var assignments = await _dbContext.Set<UserRole>()
            .AsNoTracking()
            .Include(ur => ur.Role)
                .ThenInclude(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
            .Where(ur => ur.UserId == userId &&
                         ur.ValidFrom <= DateTime.UtcNow.Date &&
                         (ur.ValidTo == null || ur.ValidTo >= DateTime.UtcNow.Date))
            .ToListAsync(cancellationToken);

        foreach (var assignment in assignments)
        {
            var role = assignment.Role;
            // Direct permissions
            foreach (var rp in role.RolePermissions)
                permissionSet.Add(rp.Permission.Key);

            // Inherited permissions
            var baseRoleId = role.BaseRoleId;
            while (baseRoleId.HasValue)
            {
                var baseRole = await _dbContext.Set<Role>()
                    .AsNoTracking()
                    .Include(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
                    .FirstOrDefaultAsync(r => r.Id == baseRoleId.Value, cancellationToken);

                if (baseRole == null) break;

                foreach (var rp in baseRole.RolePermissions)
                    permissionSet.Add(rp.Permission.Key);

                baseRoleId = baseRole.BaseRoleId;
            }
        }

        return permissionSet;
    }
}