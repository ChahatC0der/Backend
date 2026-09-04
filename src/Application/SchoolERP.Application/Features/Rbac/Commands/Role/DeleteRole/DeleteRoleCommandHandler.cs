using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Extensions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Rbac.Constants;
using SchoolERP.Domain.Rbac.Entities;
using SchoolERP.Domain.Shared.Results;
using RoleEntity = SchoolERP.Domain.Rbac.Entities.Role;

namespace SchoolERP.Application.Features.Rbac.Commands.Role.DeleteRole;

public class DeleteRoleCommandHandler : IRequestHandler<DeleteRoleCommand, Result<bool>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentTenantService _tenantService;
    private readonly ICurrentUserService _currentUserService;

    public DeleteRoleCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentTenantService tenantService,
        ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _tenantService = tenantService;
        _currentUserService = currentUserService;
    }

    public async Task<Result<bool>> Handle(DeleteRoleCommand command, CancellationToken cancellationToken)
    {
        var tenantId = _tenantService.GetTenantId();

        var role = await _dbContext.Set<RoleEntity>()
            .FirstOrDefaultAsync(r => r.Id == command.RoleId && r.TenantId == tenantId && !r.IsDeleted, cancellationToken);

        if (role == null)
            return Error.NotFound("Role", command.RoleId.ToString());

        if (role.IsSystem)
            return Error.Conflict("System roles cannot be deleted.");

        // Capture old values for audit
        var oldValues = System.Text.Json.JsonSerializer.Serialize(new
        {
            role.Id,
            role.Name,
            role.Code,
            role.IsBuiltin,
            role.IsSystem
        });

        // Soft delete
        role.IsDeleted = true;
        role.DeletedAt = DateTime.UtcNow;

        // Audit log
        var auditLog = new RbacAuditLog
        {
            TenantId = tenantId,
            PerformedBy = _currentUserService.GetUserId() ?? 0,
            Resource = AuditResources.Role,
            Action = AuditActions.Delete,
            AffectedRoleId = role.Id,
            OldValues = oldValues,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Set<RbacAuditLog>().Add(auditLog);

        // SaveChanges called by TransactionBehavior

        return Result.Success(true);
    }
}