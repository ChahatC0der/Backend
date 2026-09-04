using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Extensions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Rbac.Constants;
using SchoolERP.Application.Features.Rbac.DTOs;
using SchoolERP.Domain.Shared.Results;
using PermissionEntity = SchoolERP.Domain.Rbac.Entities.Permission;
using RbacAuditLogEntity = SchoolERP.Domain.Rbac.Entities.RbacAuditLog;

namespace SchoolERP.Application.Features.Rbac.Commands.Permission.UpdatePermission;

public class UpdatePermissionCommandHandler : IRequestHandler<UpdatePermissionCommand, Result<PermissionResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentTenantService _tenantService;
    private readonly ICurrentUserService _currentUserService;

    public UpdatePermissionCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentTenantService tenantService,
        ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _tenantService = tenantService;
        _currentUserService = currentUserService;
    }

    public async Task<Result<PermissionResponse>> Handle(UpdatePermissionCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var tenantId = _tenantService.GetTenantId();

        var permission = await _dbContext.Set<PermissionEntity>()
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (permission == null)
            return Error.NotFound("Permission", request.Id.ToString());

        // Uniqueness on Key (excluding current)
        var conflictError = await _dbContext.EnsureUniqueAsync<PermissionEntity>(
            p => p.Key == request.Key && p.Id != request.Id ,
            $"Permission with key '{request.Key}' already exists.",
            cancellationToken);
        if (conflictError != null)
            return conflictError;

        var oldValues = System.Text.Json.JsonSerializer.Serialize(new
        {
            permission.Id,
            permission.Key,
            permission.Action,
            permission.ModuleId,
            permission.Description
        });

        permission.ModuleId = request.ModuleId;
        permission.Action = request.Action;
        permission.Key = request.Key;
        permission.Description = request.Description;

        var auditLog = new RbacAuditLogEntity
        {
            TenantId = tenantId,
            PerformedBy = _currentUserService.GetUserId() ?? 0,
            Resource = AuditResources.Permission,
            Action = AuditActions.Update,
            OldValues = oldValues,
            NewValues = System.Text.Json.JsonSerializer.Serialize(new
            {
                permission.Id,
                permission.Key,
                permission.Action,
                permission.ModuleId,
                permission.Description
            }),
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Set<RbacAuditLogEntity>().Add(auditLog);

        // SaveChanges by TransactionBehavior

        return Result.Success(permission.Adapt<PermissionResponse>());
    }
}