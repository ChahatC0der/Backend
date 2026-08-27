using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Features.Rbac.DTOs;
using SchoolERP.Application.Common.Extensions;
using SchoolERP.Application.Common.Interfaces;
using PermissionEntity = SchoolERP.Domain.Rbac.Entities.Permission;
using RolePermissionEntity = SchoolERP.Domain.Rbac.Entities.RolePermission;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Rbac.Commands.Role.UpdateRole;

public class UpdateRoleCommandHandler : IRequestHandler<UpdateRoleCommand, Result<RoleResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentTenantService _tenantService;

    public UpdateRoleCommandHandler(IApplicationDbContext dbContext, ICurrentTenantService tenantService)
    {
        _dbContext = dbContext;
        _tenantService = tenantService;
    }

    public async Task<Result<RoleResponse>> Handle(UpdateRoleCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var tenantId = _tenantService.GetTenantId();

        var roleResult = await _dbContext.GetEntityAsync<SchoolERP.Domain.Rbac.Entities.Role>(
            r => r.Id == request.Id && r.TenantId == tenantId && !r.IsDeleted,
            "Role", request.Id.ToString(), cancellationToken);

        if (roleResult.IsFailure)
            return roleResult.Error;

        var role = roleResult.Value;
        if (role.IsSystem)
            return Error.Conflict("System roles cannot be modified.");

        // Update scalar properties
        role.Name = request.Name;
        role.Code = request.Code.Trim().ToUpperInvariant();
        role.Description = request.Description;
        role.BaseRoleId = request.BaseRoleId;

        // Replace permissions
        var existingPerms = await _dbContext.Set<RolePermissionEntity>()
            .Where(rp => rp.RoleId == role.Id)
            .ToListAsync(cancellationToken);
        _dbContext.Set<RolePermissionEntity>().RemoveRange(existingPerms);

        if (request.PermissionIds?.Any() == true)
        {
            foreach (var permId in request.PermissionIds.Distinct())
            {
                role.RolePermissions.Add(new RolePermissionEntity
                {
                    PermissionId = permId,
                    TenantId = tenantId
                });
            }
        }

        // SaveChanges will be called by TransactionBehavior

        // Map to response
        var response = role.Adapt<RoleResponse>();
        var permIds = role.RolePermissions.Select(rp => rp.PermissionId).ToList();
        var perms = await _dbContext.Set<PermissionEntity>()
            .Where(p => permIds.Contains(p.Id))
            .ProjectToType<PermissionResponse>()
            .ToListAsync(cancellationToken);
        response = response with { Permissions = perms };

        return Result.Success(response);
    }
}