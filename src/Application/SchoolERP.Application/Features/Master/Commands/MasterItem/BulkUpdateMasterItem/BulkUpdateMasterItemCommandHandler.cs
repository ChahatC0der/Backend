using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Extensions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Domain.Master.Entities;
using SchoolERP.Domain.Rbac.Entities;
using SchoolERP.Domain.Shared.Results;
using MasterCategoryEntity = SchoolERP.Domain.Master.Entities.MasterCategory;
using MasterItemEntity = SchoolERP.Domain.Master.Entities.MasterItem;

namespace SchoolERP.Application.Features.Master.Commands.MasterItem.BulkUpdateMasterItem;

public class BulkUpdateMasterItemCommandHandler : IRequestHandler<BulkUpdateMasterItemCommand, Result<bool>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentTenantService _tenantService;
    private readonly ICurrentUserService _currentUserService;

    public BulkUpdateMasterItemCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentTenantService tenantService,
        ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _tenantService = tenantService;
        _currentUserService = currentUserService;
    }

    public async Task<Result<bool>> Handle(BulkUpdateMasterItemCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var tenantId = _tenantService.GetTenantId();

        // Check category accessible
        var category = await _dbContext.Set<MasterCategoryEntity>()
            .FirstOrDefaultAsync(c => c.Id == request.CategoryId && !c.IsDeleted, cancellationToken);
        if (category == null)
            return Error.NotFound("MasterCategory", request.CategoryId.ToString());
        if (category.TenantId.HasValue && category.TenantId.Value != tenantId)
            return Error.NotFound("MasterCategory", request.CategoryId.ToString());

        var entities = await _dbContext.Set<MasterItemEntity>()
            .Where(mi => request.Ids.Contains(mi.Id) && !mi.IsDeleted)
            .ToListAsync(cancellationToken);
        if (entities.Count != request.Ids.Count)
            return Error.NotFound("MasterItem", request.Ids.ToString());

        // Ensure all items belong to accessible categories
        var categoryIds = entities.Select(mi => mi.CategoryId).Distinct();
        var accessibleCategories = await _dbContext.Set<MasterCategoryEntity>()
            .Where(c => categoryIds.Contains(c.Id) && (c.TenantId == null || c.TenantId == tenantId))
            .Select(c => c.Id)
            .ToListAsync(cancellationToken);
        if (accessibleCategories.Count != categoryIds.Count())
            return Error.NotFound("MasterItem", request.Ids.ToString());

        // Check uniqueness among selected with same new category+value
        var outsideDuplicate = await _dbContext.EnsureUniqueAsync<MasterItemEntity>(
            mi => mi.CategoryId == request.CategoryId && mi.Value == request.Value && !request.Ids.Contains(mi.Id) && !mi.IsDeleted,
            $"Another master item with value '{request.Value}' already exists in this category.",
            cancellationToken);
        if (outsideDuplicate != null) return outsideDuplicate;

        foreach (var item in entities)
        {
            item.CategoryId = request.CategoryId;
            item.Value = request.Value;
            item.Code = request.Code;
            item.Description = request.Description;
            item.Metadata = request.Metadata;
            item.SortOrder = request.SortOrder;
            item.IsSystem = request.IsSystem;
            item.IsActive = request.IsActive;
            item.UpdatedAt = DateTime.UtcNow;
        }

        var audit = new RbacAuditLog
        {
            TenantId = tenantId,
            PerformedBy = _currentUserService.GetUserId() ?? 0,
            Resource = "MasterItem",
            Action = "bulk_update",
            NewValues = System.Text.Json.JsonSerializer.Serialize(new { request.Ids, request.CategoryId, request.Value, request.Code, request.Description, request.Metadata, request.SortOrder, request.IsSystem, request.IsActive }),
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Set<RbacAuditLog>().Add(audit);

        return Result.Success(true);
    }
}