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

namespace SchoolERP.Application.Features.Rbac.Commands.Role.CreateRole;

public class CreateRoleCommandHandler : IRequestHandler<CreateRoleCommand, Result<RoleResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentTenantService _tenantService;

    public CreateRoleCommandHandler(IApplicationDbContext dbContext, ICurrentTenantService tenantService)
    {
        _dbContext = dbContext;
        _tenantService = tenantService;
    }

    public async Task<Result<RoleResponse>> Handle(CreateRoleCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var tenantId = _tenantService.GetTenantId();

        // Uniqueness check
        var conflictError = await _dbContext.EnsureUniqueAsync<RoleEntity>(
            r => r.TenantId == tenantId && r.Code == request.Code && !r.IsDeleted,
            $"Role with code '{request.Code}' already exists.",
            cancellationToken);

        if (conflictError != null)
            return conflictError;

        // Map request to entity
        var role = request.Adapt<RoleEntity>();
        role.TenantId = tenantId;
        role.IsSystem = false; // custom role

        // Assign permissions if provided
        if (request.PermissionIds?.Any() == true)
        {
            foreach (var permId in request.PermissionIds.Distinct())
            {
                role.RolePermissions.Add(new RolePermission
                {
                    PermissionId = permId,
                    TenantId = tenantId
                });
            }
        }

        _dbContext.Set<RoleEntity>().Add(role);
        // SaveChanges will be called by TransactionBehavior

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