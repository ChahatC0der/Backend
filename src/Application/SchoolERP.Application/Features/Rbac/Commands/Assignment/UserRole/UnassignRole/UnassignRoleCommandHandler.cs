using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Rbac.Constants;
using SchoolERP.Domain.Shared.Results;
using RbacAuditLogEntity = SchoolERP.Domain.Rbac.Entities.RbacAuditLog;
using UserRoleEntity = SchoolERP.Domain.Rbac.Entities.UserRole;

namespace SchoolERP.Application.Features.Rbac.Commands.Assignment.UnassignRole;

public class UnassignRoleCommandHandler : IRequestHandler<UnassignRoleCommand, Result<bool>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentTenantService _tenantService;
    private readonly ICurrentUserService _currentUserService;

    public UnassignRoleCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentTenantService tenantService,
        ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _tenantService = tenantService;
        _currentUserService = currentUserService;
    }

    public async Task<Result<bool>> Handle(UnassignRoleCommand command, CancellationToken cancellationToken)
    {
        var tenantId = _tenantService.GetTenantId();

        // Fetch userRole with Role to verify tenant
        var userRole = await _dbContext.Set<UserRoleEntity>()
            .Include(ur => ur.Role)   // 🔥 Role include karo
            .FirstOrDefaultAsync(ur => ur.Id == command.UserRoleId, cancellationToken);

        if (userRole == null)
            return Error.NotFound("UserRole", command.UserRoleId.ToString());

        // Tenant check: assignment is valid only if role belongs to current tenant
        if (userRole.Role.TenantId != tenantId)
            return Error.NotFound("UserRole", command.UserRoleId.ToString()); // or Forbidden

        // Capture old values for audit
        var oldValues = System.Text.Json.JsonSerializer.Serialize(new
        {
            userRole.UserId,
            userRole.RoleId,
            userRole.ScopeType,
            userRole.ScopeValue,
            userRole.ValidFrom,
            userRole.ValidTo
        });

        userRole.ValidTo = DateTime.UtcNow.Date;

        // Audit log
        var auditLog = new RbacAuditLogEntity
        {
            TenantId = tenantId,
            PerformedBy = _currentUserService.GetUserId() ?? 0,
            Resource = AuditResources.RoleAssignment,
            Action = AuditActions.Update,
            AffectedUserId = userRole.UserId,
            AffectedRoleId = userRole.RoleId,
            OldValues = oldValues,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Set<RbacAuditLogEntity>().Add(auditLog);

        // SaveChanges called by TransactionBehavior

        return Result.Success(true);
    }
}