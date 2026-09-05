using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using ClassGroupEntity = SchoolERP.Domain.Academic.Entities.ClassGroup;
using SchoolERP.Domain.Rbac.Entities;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Academic.Commands.ClassGroup.BulkDeleteClassGroup;

public class BulkDeleteClassGroupCommandHandler : IRequestHandler<BulkDeleteClassGroupCommand, Result<bool>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentTenantService _tenantService;
    private readonly ICurrentUserService _currentUserService;

    public BulkDeleteClassGroupCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentTenantService tenantService,
        ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _tenantService = tenantService;
        _currentUserService = currentUserService;
    }

    public async Task<Result<bool>> Handle(BulkDeleteClassGroupCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var branchId = _tenantService.GetBranchId();
        var tenantId = _tenantService.GetTenantId();

        var entities = await _dbContext.Set<ClassGroupEntity>()
            .Where(cg => cg.BranchId == branchId && request.Ids.Contains(cg.Id) && !cg.IsDeleted)
            .ToListAsync(cancellationToken);

        if (entities.Count != request.Ids.Count)
            return Error.NotFound("ClassGroup", request.Ids.ToString());

        foreach (var cg in entities)
        {
            cg.IsDeleted = true;
            cg.DeletedAt = DateTime.UtcNow;
            cg.UpdatedAt = DateTime.UtcNow;
        }

        var audit = new RbacAuditLog
        {
            TenantId = tenantId,
            PerformedBy = _currentUserService.GetUserId() ?? 0,
            Resource = "ClassGroup",
            Action = "bulk_delete",
            OldValues = System.Text.Json.JsonSerializer.Serialize(entities.Select(e => new { e.Id, e.Name })),
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Set<RbacAuditLog>().Add(audit);

        return Result.Success(true);
    }
}