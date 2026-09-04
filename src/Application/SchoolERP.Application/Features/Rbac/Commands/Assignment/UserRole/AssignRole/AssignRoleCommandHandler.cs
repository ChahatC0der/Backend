using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Extensions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Rbac.Constants;
using SchoolERP.Application.Features.Rbac.DTOs;
using SchoolERP.Domain.Shared.Results;
using RbacAuditLogEntity = SchoolERP.Domain.Rbac.Entities.RbacAuditLog;
using RoleEntity = SchoolERP.Domain.Rbac.Entities.Role;
using UserEntity = SchoolERP.Domain.Rbac.Entities.User;
using UserRoleEntity = SchoolERP.Domain.Rbac.Entities.UserRole;

namespace SchoolERP.Application.Features.Rbac.Commands.Assignment.AssignRole;

public class AssignRoleCommandHandler : IRequestHandler<AssignRoleCommand, Result<RoleAssignmentResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentTenantService _tenantService;
    private readonly ICurrentUserService _currentUserService;

    public AssignRoleCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentTenantService tenantService,
        ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _tenantService = tenantService;
        _currentUserService = currentUserService;
    }

    public async Task<Result<RoleAssignmentResponse>> Handle(AssignRoleCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var tenantId = _tenantService.GetTenantId();

        // Check user exists
        var userExists = await _dbContext.Set<UserEntity>()
            .AnyAsync(u => u.Id == request.UserId && !u.IsDeleted, cancellationToken);
        if (!userExists)
            return Error.NotFound("User", request.UserId.ToString());

        // Check role exists
        var role = await _dbContext.Set<RoleEntity>()
            .FirstOrDefaultAsync(r => r.Id == request.RoleId && r.TenantId == tenantId && !r.IsDeleted, cancellationToken);
        if (role == null)
            return Error.NotFound("Role", request.RoleId.ToString());

        // Duplicate assignment check
        var duplicateExists = await _dbContext.Set<UserRoleEntity>()
            .AnyAsync(ur => ur.UserId == request.UserId && ur.RoleId == request.RoleId &&
                            ur.ScopeType == request.ScopeType && ur.ScopeValue == request.ScopeValue, cancellationToken);
        if (duplicateExists)
            return Error.Conflict("User already has this role for the given scope.");

        var userRole = request.Adapt<UserRoleEntity>();
        userRole.ValidFrom = request.ValidFrom ?? DateTime.UtcNow.Date;
        // TenantId auto-stamped on SaveChanges, no manual assignment

        _dbContext.Set<UserRoleEntity>().Add(userRole);

        // Audit log
        var auditLog = new RbacAuditLogEntity
        {
            TenantId = tenantId,
            PerformedBy = _currentUserService.GetUserId() ?? 0,
            Resource = AuditResources.RoleAssignment,
            Action = AuditActions.Assign,
            AffectedUserId = request.UserId,
            AffectedRoleId = request.RoleId,
            NewValues = System.Text.Json.JsonSerializer.Serialize(new
            {
                request.ScopeType,
                request.ScopeValue,
                request.ValidFrom,
                request.ValidTo
            }),
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Set<RbacAuditLogEntity>().Add(auditLog);

        // Build response
        var response = userRole.Adapt<RoleAssignmentResponse>();
        response = response with { RoleName = role.Name, RoleCode = role.Code };

        return Result.Success(response);
    }
}