using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Extensions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Rbac.Constants;
using SchoolERP.Domain.Shared.Results;
using PermissionEntity = SchoolERP.Domain.Rbac.Entities.Permission;
using RbacAuditLogEntity = SchoolERP.Domain.Rbac.Entities.RbacAuditLog;

namespace SchoolERP.Application.Features.Rbac.Commands.Permission.DeletePermission;

public class DeletePermissionCommandHandler : IRequestHandler<DeletePermissionCommand, Result<bool>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentTenantService _tenantService;
    private readonly ICurrentUserService _currentUserService;

    public DeletePermissionCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentTenantService tenantService,
        ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _tenantService = tenantService;
        _currentUserService = currentUserService;
    }

    public async Task<Result<bool>> Handle(DeletePermissionCommand command, CancellationToken cancellationToken)
    {
        var tenantId = _tenantService.GetTenantId();

        var permission = await _dbContext.Set<PermissionEntity>()
            .FirstOrDefaultAsync(p => p.Id == command.PermissionId, cancellationToken);

        if (permission == null)
            return Error.NotFound("Permission", command.PermissionId.ToString());

        var oldValues = System.Text.Json.JsonSerializer.Serialize(new
        {
            permission.Id,
            permission.Key,
            permission.Action,
            permission.ModuleId
        });

        //permission.IsDeleted = true;
        //permission.DeletedAt = DateTime.UtcNow;

        var auditLog = new RbacAuditLogEntity
        {
            TenantId = tenantId,
            PerformedBy = _currentUserService.GetUserId() ?? 0,
            Resource = AuditResources.Permission,
            Action = AuditActions.Delete,
            OldValues = oldValues,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Set<RbacAuditLogEntity>().Add(auditLog);

        // SaveChanges by TransactionBehavior

        return Result.Success(true);
    }
}