using MediatR;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Common.Extensions;
using SchoolERP.Domain.Rbac.Entities;
using SchoolERP.Domain.Shared.Results;

public class DeletePermissionCommandHandler : IRequestHandler<DeletePermissionCommand, Result<bool>>
{
    private readonly IApplicationDbContext _dbContext;
    public DeletePermissionCommandHandler(IApplicationDbContext dbContext) => _dbContext = dbContext;

    public async Task<Result<bool>> Handle(DeletePermissionCommand command, CancellationToken cancellationToken)
    {
        var permissionResult = await _dbContext.GetEntityAsync<Permission>(
            p => p.Id == command.PermissionId && !p.IsDeleted,
            "Permission", command.PermissionId.ToString(), cancellationToken);
        if (permissionResult.IsFailure) return permissionResult.Error;

        var permission = permissionResult.Value;
        permission.IsDeleted = true;
        permission.DeletedAt = DateTime.UtcNow;
        // SaveChanges by TransactionBehavior

        return Result.Success(true);
    }
}