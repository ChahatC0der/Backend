using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Rbac.Constants;
using SchoolERP.Application.Features.Rbac.DTOs;
using SchoolERP.Domain.Shared.Results;
using RbacAuditLogEntity = SchoolERP.Domain.Rbac.Entities.RbacAuditLog;
using UserEntity = SchoolERP.Domain.Rbac.Entities.User;

namespace SchoolERP.Application.Features.Rbac.Commands.User.UpdateUser;

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, Result<UserResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentTenantService _tenantService;
    private readonly ICurrentUserService _currentUserService;

    public UpdateUserCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentTenantService tenantService,
        ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _tenantService = tenantService;
        _currentUserService = currentUserService;
    }

    public async Task<Result<UserResponse>> Handle(UpdateUserCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var tenantId = _tenantService.GetTenantId();

        var user = await _dbContext.Set<UserEntity>()
            .FirstOrDefaultAsync(u => u.Id == request.Id && !u.IsDeleted, cancellationToken);
        if (user == null)
            return Error.NotFound("User", request.Id.ToString());

        // Capture old values for audit
        var oldValues = System.Text.Json.JsonSerializer.Serialize(new
        {
            user.Name,
            user.Phone,
            user.Status
        });

        // Update allowed fields (TenantId/BranchId are managed by system, not via this request)
        user.Name = request.Name;
        user.Phone = request.Phone;
        user.Status = request.Status ?? user.Status;
        user.UpdatedAt = DateTime.UtcNow;

        // Audit log
        var auditLog = new RbacAuditLogEntity
        {
            TenantId = tenantId,
            PerformedBy = _currentUserService.GetUserId() ?? 0,
            Resource = AuditResources.User,
            Action = AuditActions.Update,
            AffectedUserId = user.Id,
            OldValues = oldValues,
            NewValues = System.Text.Json.JsonSerializer.Serialize(new
            {
                user.Name,
                user.Phone,
                user.Status
            }),
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Set<RbacAuditLogEntity>().Add(auditLog);

        // SaveChanges handled by TransactionBehavior

        return Result.Success(user.Adapt<UserResponse>());
    }
}