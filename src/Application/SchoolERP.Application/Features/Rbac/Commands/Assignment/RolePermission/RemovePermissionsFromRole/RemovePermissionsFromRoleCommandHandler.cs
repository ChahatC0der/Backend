using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Extensions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Rbac.Constants;
using SchoolERP.Domain.Shared.Results;
using PermissionEntity = SchoolERP.Domain.Rbac.Entities.Permission;
using RbacAuditLogEntity = SchoolERP.Domain.Rbac.Entities.RbacAuditLog;
using RoleEntity = SchoolERP.Domain.Rbac.Entities.Role;
using RolePermissionEntity = SchoolERP.Domain.Rbac.Entities.RolePermission;

namespace SchoolERP.Application.Features.Rbac.Commands.Assignment.RolePermission.RemovePermissionsFromRole;

public class RemovePermissionsFromRoleCommandHandler : IRequestHandler<RemovePermissionsFromRoleCommand, Result<bool>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentTenantService _tenantService;
    private readonly ICurrentUserService _currentUserService;

    public RemovePermissionsFromRoleCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentTenantService tenantService,
        ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _tenantService = tenantService;
        _currentUserService = currentUserService;
    }

    public async Task<Result<bool>> Handle(RemovePermissionsFromRoleCommand command, CancellationToken cancellationToken)
    {
        var tenantId = _tenantService.GetTenantId();

        // Check role exists
        var role = await _dbContext.Set<RoleEntity>()
            .FirstOrDefaultAsync(r => r.Id == command.RoleId && r.TenantId == tenantId && !r.IsDeleted, cancellationToken);
        if (role == null)
            return Error.NotFound("Role", command.RoleId.ToString());

        // Existing permissions (before removal) for audit
        var existingPermIds = await _dbContext.Set<RolePermissionEntity>()
            .Where(rp => rp.RoleId == command.RoleId)
            .Select(rp => rp.PermissionId)
            .OrderBy(x => x)
            .ToListAsync(cancellationToken);

        // Remove specified permissions
        var toRemove = await _dbContext.Set<RolePermissionEntity>()
            .Where(rp => rp.RoleId == command.RoleId && command.PermissionIds.Contains(rp.PermissionId))
            .ToListAsync(cancellationToken);
        _dbContext.Set<RolePermissionEntity>().RemoveRange(toRemove);

        // Final permission list after removal
        var finalPermIds = existingPermIds.Except(command.PermissionIds).Distinct().OrderBy(x => x).ToList();

        // Audit log
        var auditLog = new RbacAuditLogEntity
        {
            TenantId = tenantId,
            PerformedBy = _currentUserService.GetUserId() ?? 0,
            Resource = AuditResources.Permission,
            Action = AuditActions.Update,
            AffectedRoleId = command.RoleId,
            OldValues = System.Text.Json.JsonSerializer.Serialize(existingPermIds),
            NewValues = System.Text.Json.JsonSerializer.Serialize(finalPermIds),
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Set<RbacAuditLogEntity>().Add(auditLog);

        // SaveChanges by TransactionBehavior
        return Result.Success(true);
    }
}