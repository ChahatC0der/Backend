using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Extensions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Master.DTOs;
using SchoolERP.Domain.Master.Entities;
using SchoolERP.Domain.Rbac.Entities;
using SchoolERP.Domain.Shared.Results;
using MasterCategoryEntity = SchoolERP.Domain.Master.Entities.MasterCategory;
using MasterItemEntity = SchoolERP.Domain.Master.Entities.MasterItem;

namespace SchoolERP.Application.Features.Master.Commands.MasterItem.PatchMasterItem;

public class PatchMasterItemCommandHandler : IRequestHandler<PatchMasterItemCommand, Result<MasterItemResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentTenantService _tenantService;
    private readonly ICurrentUserService _currentUserService;

    public PatchMasterItemCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentTenantService tenantService,
        ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _tenantService = tenantService;
        _currentUserService = currentUserService;
    }

    public async Task<Result<MasterItemResponse>> Handle(PatchMasterItemCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var tenantId = _tenantService.GetTenantId();

        var item = await _dbContext.Set<MasterItemEntity>()
            .Include(mi => mi.Category)
            .FirstOrDefaultAsync(mi => mi.Id == request.Id && !mi.IsDeleted, cancellationToken);
        if (item == null)
            return Error.NotFound("MasterItem", request.Id.ToString());

        // Check category accessibility
        if (item.Category.TenantId.HasValue && item.Category.TenantId.Value != tenantId)
            return Error.NotFound("MasterItem", request.Id.ToString());

        var oldValues = System.Text.Json.JsonSerializer.Serialize(new { item.CategoryId, item.Value, item.Code, item.Description, item.Metadata, item.SortOrder, item.IsSystem, item.IsActive });

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

        // Uniqueness if value or category changed
        if (!string.IsNullOrWhiteSpace(request.Value) || request.CategoryId.HasValue)
        {
            var conflict = await _dbContext.EnsureUniqueAsync<MasterItemEntity>(
                mi => mi.CategoryId == item.CategoryId && mi.Value == item.Value && mi.Id != item.Id && !mi.IsDeleted,
                $"Master item '{item.Value}' already exists in this category.",
                cancellationToken);
            if (conflict != null) return conflict;
        }

        item.UpdatedAt = DateTime.UtcNow;

        var audit = new RbacAuditLog
        {
            TenantId = tenantId,
            PerformedBy = _currentUserService.GetUserId() ?? 0,
            Resource = "MasterItem",
            Action = "patch",
            OldValues = oldValues,
            NewValues = System.Text.Json.JsonSerializer.Serialize(new { item.CategoryId, item.Value, item.Code, item.Description, item.Metadata, item.SortOrder, item.IsSystem, item.IsActive }),
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Set<RbacAuditLog>().Add(audit);

        return Result.Success(item.Adapt<MasterItemResponse>());
    }
}