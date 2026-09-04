using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Rbac.Constants;
using SchoolERP.Application.Features.Rbac.DTOs;
using SchoolERP.Domain.Shared.Results;
using RbacAuditLogEntity = SchoolERP.Domain.Rbac.Entities.RbacAuditLog;
using UserEntity = SchoolERP.Domain.Rbac.Entities.User;

namespace SchoolERP.Application.Features.Rbac.Commands.User.CreateUser;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Result<UserResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentTenantService _tenantService;
    private readonly ICurrentUserService _currentUserService;

    public CreateUserCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentTenantService tenantService,
        ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _tenantService = tenantService;
        _currentUserService = currentUserService;
    }

    public async Task<Result<UserResponse>> Handle(CreateUserCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var tenantId = _tenantService.GetTenantId();

        // Email uniqueness check
        var exists = await _dbContext.Set<UserEntity>()
            .AnyAsync(u => u.Email == request.Email && !u.IsDeleted, cancellationToken);
        if (exists)
            return Error.Conflict($"User with email '{request.Email}' already exists.");

        var user = request.Adapt<UserEntity>();
        // TODO: Replace with proper password hashing (e.g., BCrypt, Identity PasswordHasher) later
        user.PasswordHash = request.Password; // temporary plain text
        user.TenantId = tenantId; // set current tenant (if user entity has TenantId)
        user.PermissionsVersion = 1;
        user.CreatedAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;

        _dbContext.Set<UserEntity>().Add(user);

        // Audit log
        var auditLog = new RbacAuditLogEntity
        {
            TenantId = tenantId,
            PerformedBy = _currentUserService.GetUserId() ?? 0,
            Resource = AuditResources.User,
            Action = AuditActions.Create,
            AffectedUserId = user.Id, // will be 0 until save; acceptable for now
            NewValues = System.Text.Json.JsonSerializer.Serialize(new
            {
                user.Name,
                user.Email,
                user.Phone,
                user.IsPlatformAdmin,
                user.Status
            }),
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Set<RbacAuditLogEntity>().Add(auditLog);

        // SaveChanges handled by TransactionBehavior

        return Result.Success(user.Adapt<UserResponse>());
    }
}