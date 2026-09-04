using Mapster;
using MediatR;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Rbac.DTOs;
using SchoolERP.Domain.Shared.Results;
using BulkRoleJobEntity = SchoolERP.Domain.Rbac.Entities.BulkRoleJob;
using RbacAuditLogEntity = SchoolERP.Domain.Rbac.Entities.RbacAuditLog;

namespace SchoolERP.Application.Features.Rbac.Commands.Assignment.BulkAssignRole;

public class BulkAssignRoleCommandHandler : IRequestHandler<BulkAssignRoleCommand, Result<BulkRoleJobResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentTenantService _tenantService;
    private readonly ICurrentUserService _currentUserService;

    public BulkAssignRoleCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentTenantService tenantService,
        ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _tenantService = tenantService;
        _currentUserService = currentUserService;
    }

    public async Task<Result<BulkRoleJobResponse>> Handle(BulkAssignRoleCommand command, CancellationToken cancellationToken)
    {
        var tenantId = _tenantService.GetTenantId();

        // Create a bulk job record (processing would be done via background job)
        var job = new BulkRoleJobEntity
        {
            // TenantId auto-stamped, no manual assignment
            CreatedBy = _currentUserService.GetUserId() ?? 0,
            RoleId = command.RoleId,
            ScopeType = command.ScopeType,
            ScopeValue = command.ScopeValue ?? string.Empty,
            TotalUsers = command.UserIds.Count,
            Status = "pending"
        };

        _dbContext.Set<BulkRoleJobEntity>().Add(job);

        // Audit log for job creation (optional)
        var auditLog = new RbacAuditLogEntity
        {
            TenantId = tenantId,
            PerformedBy = _currentUserService.GetUserId() ?? 0,
            Action = "bulk_role_assignment_created",
            NewValues = System.Text.Json.JsonSerializer.Serialize(new
            {
                job.RoleId,
                job.ScopeType,
                job.ScopeValue,
                job.TotalUsers
            }),
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Set<RbacAuditLogEntity>().Add(auditLog);

        // SaveChanges called by TransactionBehavior

        return Result.Success(job.Adapt<BulkRoleJobResponse>());
    }
}