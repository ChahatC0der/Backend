using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Extensions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Rbac.DTOs;
using SchoolERP.Domain.Rbac.Entities;
using SchoolERP.Domain.Shared.Results;
using RoleEntity = SchoolERP.Domain.Rbac.Entities.Role;
using UserEntity = SchoolERP.Domain.Rbac.Entities.User;
using UserRoleEntity = SchoolERP.Domain.Rbac.Entities.UserRole;

namespace SchoolERP.Application.Features.Rbac.Commands.Assignment.AssignRole;

public class AssignRoleCommandHandler : IRequestHandler<AssignRoleCommand, Result<RoleAssignmentResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentTenantService _tenantService;
    private readonly ICurrentUserService _currentUserService;

    public AssignRoleCommandHandler(IApplicationDbContext dbContext, ICurrentTenantService tenantService, ICurrentUserService currentUserService)
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
        var userExistsError = await _dbContext.EnsureEntityExistsAsync<UserEntity>(request.UserId, cancellationToken);
        if (userExistsError != null)
            return userExistsError;

        // Check role exists (using GetEntityAsync for proper NotFound)
        var roleResult = await _dbContext.GetEntityAsync<RoleEntity>(
            r => r.Id == request.RoleId && r.TenantId == tenantId && !r.IsDeleted,
            "Role", request.RoleId.ToString(), cancellationToken);
        if (roleResult.IsFailure)
            return roleResult.Error;

        // Duplicate assignment check
        var duplicateError = await _dbContext.EnsureUniqueAsync<UserRoleEntity>(
            ur => ur.UserId == request.UserId && ur.RoleId == request.RoleId &&
                  ur.ScopeType == request.ScopeType && ur.ScopeValue == request.ScopeValue,
            "User already has this role for the given scope.",
            cancellationToken);
        if (duplicateError != null)
            return duplicateError;

        var userRole = request.Adapt<UserRoleEntity>();
        userRole.TenantId = tenantId;
        userRole.ValidFrom = request.ValidFrom ?? DateTime.UtcNow.Date;

        _dbContext.Set<UserRoleEntity>().Add(userRole);

        // SaveChanges called by TransactionBehavior
        // ---- Audit log manual add ----
        var auditLog = new RbacAuditLog
        {
            TenantId = tenantId,
            PerformedBy = _currentUserService.GetUserId() ?? 0,   // or placeholder 0
            AffectedUserId = request.UserId,
            AffectedRoleId = request.RoleId,
            Action = "role_assigned",
            NewValues = System.Text.Json.JsonSerializer.Serialize(new
            {
                request.ScopeType,
                request.ScopeValue,
                request.ValidFrom,
                request.ValidTo
            }),
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Set<RbacAuditLog>().Add(auditLog);

        // Build response with role name/code
        var role = roleResult.Value;
        var response = userRole.Adapt<RoleAssignmentResponse>();
        response = response with { RoleName = role.Name, RoleCode = role.Code };

        return Result.Success(response);
    }
}