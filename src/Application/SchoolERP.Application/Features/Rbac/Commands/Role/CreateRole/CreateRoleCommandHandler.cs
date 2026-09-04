using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Extensions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Rbac.DTOs;
using SchoolERP.Domain.Rbac.Entities;
using RoleEntity = SchoolERP.Domain.Rbac.Entities.Role;
using PermissionEntity = SchoolERP.Domain.Rbac.Entities.Permission;
using SchoolERP.Domain.Shared.Results;
using SchoolERP.Application.Features.Rbac.Constants;

namespace SchoolERP.Application.Features.Rbac.Commands.Role.CreateRole;

public class CreateRoleCommandHandler : IRequestHandler<CreateRoleCommand, Result<RoleResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentTenantService _tenantService;
    private readonly ICurrentUserService _currentUserService;

    public CreateRoleCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentTenantService tenantService,
        ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _tenantService = tenantService;
        _currentUserService = currentUserService;
    }

    public async Task<Result<RoleResponse>> Handle(CreateRoleCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var tenantId = _tenantService.GetTenantId(); // only for uniqueness check

        // Uniqueness check (requires tenantId)
        var conflictError = await _dbContext.EnsureUniqueAsync<RoleEntity>(
            r => r.TenantId == tenantId && r.Code == request.Code && !r.IsDeleted,
            $"Role with code '{request.Code}' already exists.",
            cancellationToken);

        if (conflictError != null)
            return conflictError;

        // Map request to entity (NO manual TenantId set)
        var role = request.Adapt<RoleEntity>();
        role.IsSystem = false; // custom role

        // Assign permissions if provided
        if (request.PermissionIds?.Any() == true)
        {
            var distinctPermIds = request.PermissionIds.Distinct().ToList();

            // 👉 Check all permission IDs exist
            var existingPermIds = await _dbContext.Set<PermissionEntity>()
                .Where(p => distinctPermIds.Contains(p.Id))
                .Select(p => p.Id)
                .ToListAsync(cancellationToken);

            var invalidIds = distinctPermIds.Except(existingPermIds).ToList();
            if (invalidIds.Any())
                return Error.Validation($"Invalid permission IDs: {string.Join(", ", invalidIds)}");

            foreach (var permId in distinctPermIds)
            {
                role.RolePermissions.Add(new RolePermission
                {
                    PermissionId = permId
                    // TenantId auto-stamp hoga
                });
            }
        }

        _dbContext.Set<RoleEntity>().Add(role); // SaveChanges auto sets TenantId

        //Audit log(uses tenantId from service for logging)
            var auditLog = new RbacAuditLog
            {
                TenantId = tenantId, // audit log also auto? Actually RbacAuditLog inherits TenantAuditableEntity, but we can set here for clarity; auto will set too.
                PerformedBy = _currentUserService.GetUserId() ?? 0,
                Resource = AuditResources.Role,
                Action = AuditActions.Create,
                NewValues = System.Text.Json.JsonSerializer.Serialize(new { role.Name, role.Code, request.PermissionIds }),
                CreatedAt = DateTime.UtcNow
            };
        _dbContext.Set<RbacAuditLog>().Add(auditLog);

        // Map to response (with permissions)
        var response = role.Adapt<RoleResponse>();
        if (role.RolePermissions.Any())
        {
            var permIds = role.RolePermissions.Select(rp => rp.PermissionId).ToList();
            var perms = await _dbContext.Set<PermissionEntity>()
                .Where(p => permIds.Contains(p.Id))
                .ProjectToType<PermissionResponse>()
                .ToListAsync(cancellationToken);
            response = response with { Permissions = perms };
        }

        return Result.Success(response);
    }
}