using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Rbac.DTOs;
using UserEntity = SchoolERP.Domain.Rbac.Entities.User;
using RoleEntity = SchoolERP.Domain.Rbac.Entities.Role;
using UserRoleEntity = SchoolERP.Domain.Rbac.Entities.UserRole;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Rbac.Queries.User.GetUserPermissions;

public class GetUserPermissionsQueryHandler : IRequestHandler<GetUserPermissionsQuery, Result<UserPermissionsResponse>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetUserPermissionsQueryHandler(IApplicationDbContext dbContext) => _dbContext = dbContext;

    public async Task<Result<UserPermissionsResponse>> Handle(GetUserPermissionsQuery query, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Set<UserEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == query.UserId && !u.IsDeleted, cancellationToken);

        if (user == null)
            return Error.NotFound("User", query.UserId.ToString());

        // Platform Admin gets wildcard
        if (user.IsPlatformAdmin)
        {
            return Result.Success(new UserPermissionsResponse(
                user.Id,
                user.TenantId ?? Guid.Empty,
                user.BranchId,
                user.PermissionsVersion,
                new List<RoleAssignmentResponse>(),
                new List<string> { "*.*" }
            ));
        }

        var assignments = await _dbContext.Set<UserRoleEntity>()
            .AsNoTracking()
            .Include(ur => ur.Role)
                .ThenInclude(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
            .Where(ur => ur.UserId == query.UserId &&
                         ur.ValidFrom <= DateTime.UtcNow.Date &&
                         (ur.ValidTo == null || ur.ValidTo >= DateTime.UtcNow.Date))
            .ToListAsync(cancellationToken);

        var roles = assignments.Select(ur => ur.Adapt<RoleAssignmentResponse>()).ToList();

        var permissionSet = new HashSet<string>();
        foreach (var assignment in assignments)
        {
            var role = assignment.Role;
            foreach (var rp in role.RolePermissions)
                permissionSet.Add(rp.Permission.Key);

            // Handle inheritance
            var baseRoleId = role.BaseRoleId;
            while (baseRoleId.HasValue)
            {
                var baseRole = await _dbContext.Set<RoleEntity>()
                    .AsNoTracking()
                    .Include(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
                    .FirstOrDefaultAsync(r => r.Id == baseRoleId.Value && !r.IsDeleted, cancellationToken);
                if (baseRole == null) break;
                foreach (var rp in baseRole.RolePermissions)
                    permissionSet.Add(rp.Permission.Key);
                baseRoleId = baseRole.BaseRoleId;
            }
        }

        return Result.Success(new UserPermissionsResponse(
            user.Id,
            user.TenantId ?? Guid.Empty,
            user.BranchId,
            user.PermissionsVersion,
            roles,
            permissionSet.ToList()
        ));
    }
}