using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Extensions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Rbac.Constants;
using SchoolERP.Application.Features.Rbac.DTOs;
using SchoolERP.Domain.Rbac.Entities;
using SchoolERP.Domain.Shared.Results;
using PermissionEntity = SchoolERP.Domain.Rbac.Entities.Permission;
using RoleEntity = SchoolERP.Domain.Rbac.Entities.Role;
using RolePermissionEntity = SchoolERP.Domain.Rbac.Entities.RolePermission;

namespace SchoolERP.Application.Features.Rbac.Commands.Role.UpdateRole;

public class UpdateRoleCommandHandler : IRequestHandler<UpdateRoleCommand, Result<RoleResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentTenantService _tenantService;
    private readonly ICurrentUserService _currentUserService;

    public UpdateRoleCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentTenantService tenantService,
        ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _tenantService = tenantService;
        _currentUserService = currentUserService;
    }

    public async Task<Result<RoleResponse>> Handle(UpdateRoleCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var roleId = command.id;
        var tenantId = _tenantService.GetTenantId();

        // Load role with existing permissions
        var role = await _dbContext.Set<RoleEntity>()
            .Include(r => r.RolePermissions)
            .FirstOrDefaultAsync(r => r.Id == roleId && r.TenantId == tenantId && !r.IsDeleted, cancellationToken);

        if (role == null)
            return Error.NotFound("Role", roleId.ToString());

        if (role.IsSystem)
            return Error.Conflict("System roles cannot be modified.");

        // Capture old values for audit
        var oldValues = System.Text.Json.JsonSerializer.Serialize(new
        {
            role.Name,
            role.Code,
            role.Description,
            role.BaseRoleId,
            Permissions = role.RolePermissions.Select(rp => rp.PermissionId).OrderBy(x => x).ToArray()
        });

        // Update scalar properties
        role.Name = request.Name;
        role.Code = request.Code.Trim().ToUpperInvariant();
        role.Description = request.Description;
        role.BaseRoleId = request.BaseRoleId;

        // Replace permissions
        var existingPerms = role.RolePermissions.ToList();
        _dbContext.Set<RolePermissionEntity>().RemoveRange(existingPerms);

        if (request.PermissionIds?.Any() == true)
        {
            var distinctPermIds = request.PermissionIds.Distinct().ToList();

            var existingPermIds = await _dbContext.Set<PermissionEntity>()
                .Where(p => distinctPermIds.Contains(p.Id))
                .Select(p => p.Id)
                .ToListAsync(cancellationToken);

            var invalidIds = distinctPermIds.Except(existingPermIds).ToList();
            if (invalidIds.Any())
                return Error.Validation($"Invalid permission IDs: {string.Join(", ", invalidIds)}");

            foreach (var permId in distinctPermIds)
            {
                _dbContext.Set<RolePermissionEntity>().Add(new RolePermissionEntity
                {
                    RoleId = role.Id,
                    PermissionId = permId
                });
            }
        }

        // Audit log
        var newPermissions = request.PermissionIds?.Distinct().OrderBy(x => x).ToArray() ?? Array.Empty<long>();
        var auditLog = new RbacAuditLog
        {
            TenantId = tenantId,
            PerformedBy = _currentUserService.GetUserId() ?? 0,
            Resource = AuditResources.Role,
            Action = AuditActions.Update,
            AffectedRoleId = role.Id,
            OldValues = oldValues,
            NewValues = System.Text.Json.JsonSerializer.Serialize(new
            {
                role.Name,
                role.Code,
                role.Description,
                role.BaseRoleId,
                Permissions = newPermissions
            }),
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Set<RbacAuditLog>().Add(auditLog);

        // SaveChanges will be called by TransactionBehavior

        // Map to response
        var response = role.Adapt<RoleResponse>();
        if (request.PermissionIds?.Any() == true)
        {
            var permIds = request.PermissionIds.Distinct().ToList();
            var perms = await _dbContext.Set<PermissionEntity>()
                .Where(p => permIds.Contains(p.Id))
                .ProjectToType<PermissionResponse>()
                .ToListAsync(cancellationToken);
            response = response with { Permissions = perms };
        }
        else
        {
            response = response with { Permissions = new List<PermissionResponse>() };
        }

        return Result.Success(response);
    }
}