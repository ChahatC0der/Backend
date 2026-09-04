using Mapster;
using MediatR;
using SchoolERP.Application.Common.Extensions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Rbac.Constants;
using SchoolERP.Application.Features.Rbac.DTOs;
using SchoolERP.Domain.Shared.Results;
using ModuleEntity = SchoolERP.Domain.Rbac.Entities.Module;
using PermissionEntity = SchoolERP.Domain.Rbac.Entities.Permission;
using RbacAuditLogEntity = SchoolERP.Domain.Rbac.Entities.RbacAuditLog;

namespace SchoolERP.Application.Features.Rbac.Commands.Permission.CreatePermission;

public class CreatePermissionCommandHandler : IRequestHandler<CreatePermissionCommand, Result<PermissionResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentTenantService _tenantService;
    private readonly ICurrentUserService _currentUserService;

    public CreatePermissionCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentTenantService tenantService,
        ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _tenantService = tenantService;
        _currentUserService = currentUserService;
    }

    public async Task<Result<PermissionResponse>> Handle(CreatePermissionCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var tenantId = _tenantService.GetTenantId();

        // Check module exists
        var moduleExistsError = await _dbContext.EnsureEntityExistsAsync<ModuleEntity>(request.ModuleId, cancellationToken);
        if (moduleExistsError != null) return moduleExistsError;

        // Uniqueness on Key
        var conflictError = await _dbContext.EnsureUniqueAsync<PermissionEntity>(
            p => p.Key == request.Key,
            $"Permission with key '{request.Key}' already exists.",
            cancellationToken);
        if (conflictError != null) return conflictError;

        var permission = request.Adapt<PermissionEntity>();
        _dbContext.Set<PermissionEntity>().Add(permission);

        // Audit log
        var auditLog = new RbacAuditLogEntity
        {
            TenantId = tenantId,
            PerformedBy = _currentUserService.GetUserId() ?? 0,
            Resource = AuditResources.Permission,
            Action = AuditActions.Create,
            NewValues = System.Text.Json.JsonSerializer.Serialize(new
            {
                permission.Key,
                permission.Action,
                permission.ModuleId,
                permission.Description
            }),
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Set<RbacAuditLogEntity>().Add(auditLog);

        // SaveChanges handled by TransactionBehavior

        return Result.Success(permission.Adapt<PermissionResponse>());
    }
}