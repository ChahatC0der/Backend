using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using ClassEntity = SchoolERP.Domain.Academic.Entities.Class;
using SchoolERP.Domain.Rbac.Entities;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Academic.Commands.Class.BulkDeleteClass;

public class BulkDeleteClassCommandHandler : IRequestHandler<BulkDeleteClassCommand, Result<bool>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentTenantService _tenantService;
    private readonly ICurrentUserService _currentUserService;

    public BulkDeleteClassCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentTenantService tenantService,
        ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _tenantService = tenantService;
        _currentUserService = currentUserService;
    }

    public async Task<Result<bool>> Handle(BulkDeleteClassCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var branchId = _tenantService.GetBranchId();
        var tenantId = _tenantService.GetTenantId();

        var entities = await _dbContext.Set<ClassEntity>()
            .Where(c => c.BranchId == branchId && request.Ids.Contains(c.Id) && !c.IsDeleted)
            .ToListAsync(cancellationToken);

        if (entities.Count != request.Ids.Count)
            return Error.NotFound("Class", request.Ids.ToString());

        foreach (var c in entities)
        {
            c.IsDeleted = true;
            c.DeletedAt = DateTime.UtcNow;
            c.UpdatedAt = DateTime.UtcNow;
        }

        var audit = new RbacAuditLog
        {
            TenantId = tenantId,
            PerformedBy = _currentUserService.GetUserId() ?? 0,
            Resource = "Class",
            Action = "bulk_delete",
            OldValues = System.Text.Json.JsonSerializer.Serialize(entities.Select(e => new { e.Id, e.Name })),
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Set<RbacAuditLog>().Add(audit);

        return Result.Success(true);
    }
}