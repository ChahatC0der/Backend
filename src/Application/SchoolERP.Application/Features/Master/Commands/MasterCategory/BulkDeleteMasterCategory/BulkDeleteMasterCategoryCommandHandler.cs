using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using MasterCategoryEntity =  SchoolERP.Domain.Master.Entities.MasterCategory;
using SchoolERP.Domain.Rbac.Entities;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Master.Commands.MasterCategory.BulkDeleteMasterCategory;

public class BulkDeleteMasterCategoryCommandHandler : IRequestHandler<BulkDeleteMasterCategoryCommand, Result<bool>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentTenantService _tenantService;
    private readonly ICurrentUserService _currentUserService;

    public BulkDeleteMasterCategoryCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentTenantService tenantService,
        ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _tenantService = tenantService;
        _currentUserService = currentUserService;
    }

    public async Task<Result<bool>> Handle(BulkDeleteMasterCategoryCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var tenantId = _tenantService.GetTenantId();

        var entities = await _dbContext.Set<MasterCategoryEntity>()
            .Where(mc => mc.TenantId == tenantId && request.Ids.Contains(mc.Id) && !mc.IsDeleted)
            .ToListAsync(cancellationToken);

        if (entities.Count != request.Ids.Count)
            return Error.NotFound("MasterCategory",request.Ids.ToString());

        foreach (var mc in entities)
        {
            // System categories cannot be deleted
            if (mc.IsSystem)
                return Error.Conflict($"Category '{mc.Name}' is a system category and cannot be deleted.");

            mc.IsDeleted = true;
            mc.DeletedAt = DateTime.UtcNow;
            mc.UpdatedAt = DateTime.UtcNow;
        }

        var audit = new RbacAuditLog
        {
            TenantId = tenantId,
            PerformedBy = _currentUserService.GetUserId() ?? 0,
            Resource = "MasterCategory",
            Action = "bulk_delete",
            OldValues = System.Text.Json.JsonSerializer.Serialize(entities.Select(e => new { e.Id, e.Key, e.Name })),
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Set<RbacAuditLog>().Add(audit);

        return Result.Success(true);
    }
}