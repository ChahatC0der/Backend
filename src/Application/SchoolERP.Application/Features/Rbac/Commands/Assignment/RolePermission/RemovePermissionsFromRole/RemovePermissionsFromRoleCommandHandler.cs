using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Common.Extensions;
using SchoolERP.Domain.Rbac.Entities;
using SchoolERP.Domain.Shared.Results;

public class RemovePermissionsFromRoleCommandHandler : IRequestHandler<RemovePermissionsFromRoleCommand, Result<bool>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentTenantService _tenantService;
    public RemovePermissionsFromRoleCommandHandler(IApplicationDbContext dbContext, ICurrentTenantService tenantService)
    {
        _dbContext = dbContext;
        _tenantService = tenantService;
    }

    public async Task<Result<bool>> Handle(RemovePermissionsFromRoleCommand command, CancellationToken cancellationToken)
    {
        var tenantId = _tenantService.GetTenantId();

        // Check role exists
        var roleResult = await _dbContext.GetEntityAsync<Role>(
            r => r.Id == command.RoleId && r.TenantId == tenantId && !r.IsDeleted,
            "Role", command.RoleId.ToString(), cancellationToken);
        if (roleResult.IsFailure) return roleResult.Error;

        var toRemove = await _dbContext.Set<RolePermission>()
            .Where(rp => rp.RoleId == command.RoleId && command.PermissionIds.Contains(rp.PermissionId))
            .ToListAsync(cancellationToken);

        _dbContext.Set<RolePermission>().RemoveRange(toRemove);
        // SaveChanges by TransactionBehavior

        return Result.Success(true);
    }
}