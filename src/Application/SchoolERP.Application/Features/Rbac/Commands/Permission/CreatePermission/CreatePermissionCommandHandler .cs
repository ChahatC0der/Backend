using Mapster;
using MediatR;
using SchoolERP.Application.Common.Extensions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Rbac.DTOs;
using PermissionEntity = SchoolERP.Domain.Rbac.Entities.Permission;
using ModuleEntity = SchoolERP.Domain.Rbac.Entities.Module;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Rbac.Commands.Permission.CreatePermission;

public class CreatePermissionCommandHandler : IRequestHandler<CreatePermissionCommand, Result<PermissionResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    public CreatePermissionCommandHandler(IApplicationDbContext dbContext) => _dbContext = dbContext;

    public async Task<Result<PermissionResponse>> Handle(CreatePermissionCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;

        // Check module exists
        var moduleExistsError = await _dbContext.EnsureEntityExistsAsync<ModuleEntity>(request.ModuleId, cancellationToken);
        if (moduleExistsError != null) return moduleExistsError;

        // Uniqueness on Key
        var conflictError = await _dbContext.EnsureUniqueAsync<PermissionEntity>(
            p => p.Key == request.Key && !p.IsDeleted,
            $"Permission with key '{request.Key}' already exists.",
            cancellationToken);
        if (conflictError != null) return conflictError;

        var permission = request.Adapt<PermissionEntity>();
        _dbContext.Set<PermissionEntity>().Add(permission);
        // SaveChanges handled by TransactionBehavior

        return Result.Success(permission.Adapt<PermissionResponse>());
    }
}