using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Domain.Master.Entities;
using SchoolERP.Domain.Rbac.Entities;
using SchoolERP.Domain.Shared.Results;
using MasterItemEntity = SchoolERP.Domain.Master.Entities.MasterItem;

namespace SchoolERP.Application.Features.Master.Commands.MasterItem.BulkDeleteMasterItem;

public class BulkDeleteMasterItemCommandHandler : IRequestHandler<BulkDeleteMasterItemCommand, Result<bool>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentTenantService _tenantService;
    private readonly ICurrentUserService _currentUserService;

    public BulkDeleteMasterItemCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentTenantService tenantService,
        ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _tenantService = tenantService;
        _currentUserService = currentUserService;
    }

    public async Task<Result<bool>> Handle(BulkDeleteMasterItemCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var tenantId = _tenantService.GetTenantId();

        var entities = await _dbContext.Set<MasterItemEntity>()
            .Include(mi => mi.Category)
            .Where(mi => request.Ids.Contains(mi.Id) && !mi.IsDeleted)
            .ToListAsync(cancellationToken);
        if (entities.Count != request.Ids.Count)
            return Error.NotFound("MasterItem",request.Ids.ToString());

        foreach (var item in entities)
        {
            // Check category accessibility
            if (item.Category.TenantId.HasValue && item.Category.TenantId.Value != tenantId)
                return Error.NotFound("MasterItem", request.Ids.ToString());

            if (item.IsSystem)
                return Error.Conflict($"Item '{item.Value}' is system and cannot be deleted.");

            item.IsDeleted = true;
            item.DeletedAt = DateTime.UtcNow;
            item.UpdatedAt = DateTime.UtcNow;
        }

        var audit = new RbacAuditLog
        {
            TenantId = tenantId,
            PerformedBy = _currentUserService.GetUserId() ?? 0,
            Resource = "MasterItem",
            Action = "bulk_delete",
            OldValues = System.Text.Json.JsonSerializer.Serialize(entities.Select(e => new { e.Id, e.Value, e.Code })),
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Set<RbacAuditLog>().Add(audit);

        return Result.Success(true);
    }
}