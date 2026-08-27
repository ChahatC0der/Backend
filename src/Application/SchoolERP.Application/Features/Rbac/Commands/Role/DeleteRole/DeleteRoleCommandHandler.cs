using MediatR;
using SchoolERP.Application.Common.Extensions;
using SchoolERP.Application.Common.Interfaces;
using RoleEntity = SchoolERP.Domain.Rbac.Entities.Role;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Rbac.Commands.Role.DeleteRole;

public class DeleteRoleCommandHandler : IRequestHandler<DeleteRoleCommand, Result<bool>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentTenantService _tenantService;

    public DeleteRoleCommandHandler(IApplicationDbContext dbContext, ICurrentTenantService tenantService)
    {
        _dbContext = dbContext;
        _tenantService = tenantService;
    }

    public async Task<Result<bool>> Handle(DeleteRoleCommand command, CancellationToken cancellationToken)
    {
        var tenantId = _tenantService.GetTenantId();
        var roleResult = await _dbContext.GetEntityAsync<RoleEntity>(
            r => r.Id == command.RoleId && r.TenantId == tenantId && !r.IsDeleted,
            "Role", command.RoleId.ToString(), cancellationToken);

        if (roleResult.IsFailure)
            return roleResult.Error;

        var role = roleResult.Value;
        if (role.IsSystem)
            return Error.Conflict("System roles cannot be deleted.");

        role.IsDeleted = true;
        role.DeletedAt = DateTime.UtcNow;
        // SaveChanges called by TransactionBehavior

        return Result.Success(true);
    }
}