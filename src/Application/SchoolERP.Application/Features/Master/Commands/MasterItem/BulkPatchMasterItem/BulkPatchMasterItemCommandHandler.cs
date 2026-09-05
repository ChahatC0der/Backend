using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Extensions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Domain.Master.Entities;
using SchoolERP.Domain.Rbac.Entities;
using SchoolERP.Domain.Shared.Results;
using MasterCategoryEntity = SchoolERP.Domain.Master.Entities.MasterCategory;
using MasterItemEntity = SchoolERP.Domain.Master.Entities.MasterItem;

namespace SchoolERP.Application.Features.Master.Commands.MasterItem.BulkPatchMasterItem;

public class BulkPatchMasterItemCommandHandler : IRequestHandler<BulkPatchMasterItemCommand, Result<bool>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentTenantService _tenantService;
    private readonly ICurrentUserService _currentUserService;

    public BulkPatchMasterItemCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentTenantService tenantService,
        ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _tenantService = tenantService;
        _currentUserService = currentUserService;
    }

    public async Task<Result<bool>> Handle(BulkPatchMasterItemCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var tenantId = _tenantService.GetTenantId();

        var entities = await _dbContext.Set<MasterItemEntity>()
            .Where(mi => request.Ids.Contains(mi.Id) && !mi.IsDeleted)
            .ToListAsync(cancellationToken);
        if (entities.Count != request.Ids.Count)
            return Error.NotFound("MasterItem",request.Ids.ToString());

        // Check accessibility
        var categoryIds = entities.Select(mi => mi.CategoryId).Distinct();
        var accessibleCategories = await _dbContext.Set<MasterCategoryEntity>()
            .Where(c => categoryIds.Contains(c.Id) && (c.TenantId == null || c.TenantId == tenantId))
            .Select(c => c.Id)
            .ToListAsync(cancellationToken);
        if (accessibleCategories.Count != categoryIds.Count())
            return Error.NotFound("MasterItem", request.Ids.ToString());

        foreach (var item in entities)
        {
            if (request.CategoryId.HasValue)
            {
                var newCategory = await _dbContext.Set<MasterCategoryEntity>()
                    .FirstOrDefaultAsync(c => c.Id == request.CategoryId.Value && !c.IsDeleted, cancellationToken);
                if (newCategory == null)
                    return Error.NotFound("MasterCategory", request.CategoryId.Value.ToString());
                if (newCategory.TenantId.HasValue && newCategory.TenantId.Value != tenantId)
                    return Error.NotFound("MasterCategory", request.CategoryId.Value.ToString());
                item.CategoryId = request.CategoryId.Value;
            }

            request.Value.PatchIfProvided(value => item.Value = value);
            request.Code.PatchIfProvided(value => item.Code = value);
            if (request.Description != null) item.Description = request.Description;
            if (request.Metadata != null) item.Metadata = request.Metadata;
            if (request.SortOrder.HasValue) item.SortOrder = request.SortOrder.Value;
            if (request.IsSystem.HasValue) item.IsSystem = request.IsSystem.Value;
            if (request.IsActive.HasValue) item.IsActive = request.IsActive.Value;

            // Uniqueness check if value or category changed
            if (!string.IsNullOrWhiteSpace(request.Value) || request.CategoryId.HasValue)
            {
                var conflict = await _dbContext.EnsureUniqueAsync<MasterItemEntity>(
                    x => x.CategoryId == item.CategoryId && x.Value == item.Value && x.Id != item.Id && !x.IsDeleted,
                    $"Another master item with value '{item.Value}' already exists in this category.",
                    cancellationToken);
                if (conflict != null) return conflict;
            }

            item.UpdatedAt = DateTime.UtcNow;
        }

        var audit = new RbacAuditLog
        {
            TenantId = tenantId,
            PerformedBy = _currentUserService.GetUserId() ?? 0,
            Resource = "MasterItem",
            Action = "bulk_patch",
            NewValues = System.Text.Json.JsonSerializer.Serialize(new { request.Ids, request.CategoryId, request.Value, request.Code, request.Description, request.Metadata, request.SortOrder, request.IsSystem, request.IsActive }),
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Set<RbacAuditLog>().Add(audit);

        return Result.Success(true);
    }
}