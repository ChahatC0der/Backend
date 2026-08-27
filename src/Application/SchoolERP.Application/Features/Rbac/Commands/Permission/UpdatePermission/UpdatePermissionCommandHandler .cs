using MediatR;
using Mapster;
using SchoolERP.Application.Features.Rbac.DTOs;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Common.Extensions;
using PermissionEntity = SchoolERP.Domain.Rbac.Entities.Permission;
using SchoolERP.Domain.Shared.Results;

public class UpdatePermissionCommandHandler : IRequestHandler<UpdatePermissionCommand, Result<PermissionResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    public UpdatePermissionCommandHandler(IApplicationDbContext dbContext) => _dbContext = dbContext;

    public async Task<Result<PermissionResponse>> Handle(UpdatePermissionCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;

        var permissionResult = await _dbContext.GetEntityAsync<PermissionEntity>(
            p => p.Id == request.Id && !p.IsDeleted,
            "Permission", request.Id.ToString(), cancellationToken);
        if (permissionResult.IsFailure) return permissionResult.Error;

        var permission = permissionResult.Value;

        // Uniqueness on Key (excluding current)
        var conflictError = await _dbContext.EnsureUniqueAsync<PermissionEntity>(
            p => p.Key == request.Key && p.Id != request.Id && !p.IsDeleted,
            $"Permission with key '{request.Key}' already exists.",
            cancellationToken);
        if (conflictError != null) return conflictError;

        permission.ModuleId = request.ModuleId;
        permission.Action = request.Action;
        permission.Key = request.Key;
        permission.Description = request.Description;
        // SaveChanges by TransactionBehavior

        return Result.Success(permission.Adapt<PermissionResponse>());
    }
}