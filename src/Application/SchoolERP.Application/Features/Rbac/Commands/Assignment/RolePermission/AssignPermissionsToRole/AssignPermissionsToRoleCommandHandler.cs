using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Common.Extensions;
using SchoolERP.Domain.Rbac.Entities;
using SchoolERP.Domain.Shared.Results;

public class AssignPermissionsToRoleCommandHandler : IRequestHandler<AssignPermissionsToRoleCommand, Result<bool>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentTenantService _tenantService;
    public AssignPermissionsToRoleCommandHandler(IApplicationDbContext dbContext, ICurrentTenantService tenantService)
    {
        _dbContext = dbContext;
        _tenantService = tenantService;
    }

    public async Task<Result<bool>> Handle(AssignPermissionsToRoleCommand command, CancellationToken cancellationToken)
    {
        var tenantId = _tenantService.GetTenantId();

        // Check role exists
        var roleResult = await _dbContext.GetEntityAsync<Role>(
            r => r.Id == command.RoleId && r.TenantId == tenantId && !r.IsDeleted,
            "Role", command.RoleId.ToString(), cancellationToken);
        if (roleResult.IsFailure) return roleResult.Error;

        // Filter out already assigned permissions
        var existingPermIds = await _dbContext.Set<RolePermission>()
            .Where(rp => rp.RoleId == command.RoleId)
            .Select(rp => rp.PermissionId)
            .ToListAsync(cancellationToken);

        var newPermIds = command.PermissionIds.Distinct().Except(existingPermIds).ToList();

        foreach (var permId in newPermIds)
        {
            // Optional: check permission exists
            var permExists = await _dbContext.EnsureEntityExistsAsync<Permission>(permId, cancellationToken);
            if (permExists != null) continue; // or return error, but we skip invalid

            _dbContext.Set<RolePermission>().Add(new RolePermission
            {
                RoleId = command.RoleId,
                PermissionId = permId,
                TenantId = tenantId
            });
        }

        // SaveChanges by TransactionBehavior
        return Result.Success(true);
    }
}