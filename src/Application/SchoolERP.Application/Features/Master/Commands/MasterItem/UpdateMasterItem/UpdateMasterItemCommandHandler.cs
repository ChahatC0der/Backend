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

namespace SchoolERP.Application.Features.Master.Commands.MasterItem.UpdateMasterItem;

public class UpdateMasterItemCommandHandler : IRequestHandler<UpdateMasterItemCommand, Result<MasterItemResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentTenantService _tenantService;
    private readonly ICurrentUserService _currentUserService;

    public UpdateMasterItemCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentTenantService tenantService,
        ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _tenantService = tenantService;
        _currentUserService = currentUserService;
    }

    public async Task<Result<MasterItemResponse>> Handle(UpdateMasterItemCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var tenantId = _tenantService.GetTenantId();

        // Check category exists and accessible
        var category = await _dbContext.Set<MasterCategoryEntity>()
            .FirstOrDefaultAsync(c => c.Id == request.CategoryId && !c.IsDeleted, cancellationToken);
        if (category == null)
            return Error.NotFound("MasterCategory", request.CategoryId.ToString());
        if (category.TenantId.HasValue && category.TenantId.Value != tenantId)
            return Error.NotFound("MasterCategory", request.CategoryId.ToString());

        // Fetch item
        var item = await _dbContext.Set<MasterItemEntity>()
            .FirstOrDefaultAsync(mi => mi.Id == request.Id && !mi.IsDeleted, cancellationToken);
        if (item == null)
            return Error.NotFound("MasterItem", request.Id.ToString());

        // Uniqueness check (excluding current)
        var conflict = await _dbContext.EnsureUniqueAsync<MasterItemEntity>(
            mi => mi.CategoryId == request.CategoryId && mi.Value == request.Value && mi.Id != request.Id && !mi.IsDeleted,
            $"Master item '{request.Value}' already exists in this category.",
            cancellationToken);
        if (conflict != null) return conflict;

        var oldValues = System.Text.Json.JsonSerializer.Serialize(new { item.CategoryId, item.Value, item.Code, item.Description, item.Metadata, item.SortOrder, item.IsSystem, item.IsActive });

        item.CategoryId = request.CategoryId;
        item.Value = request.Value;
        item.Code = request.Code;
        item.Description = request.Description;
        item.Metadata = request.Metadata;
        item.SortOrder = request.SortOrder;
        item.IsSystem = request.IsSystem;
        item.IsActive = request.IsActive;
        item.UpdatedAt = DateTime.UtcNow;

        // Audit
        var audit = new RbacAuditLog
        {
            TenantId = tenantId,
            PerformedBy = _currentUserService.GetUserId() ?? 0,
            Resource = "MasterItem",
            Action = "update",
            OldValues = oldValues,
            NewValues = System.Text.Json.JsonSerializer.Serialize(new { item.CategoryId, item.Value, item.Code, item.Description, item.Metadata, item.SortOrder, item.IsSystem, item.IsActive }),
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Set<RbacAuditLog>().Add(audit);

        return Result.Success(item.Adapt<MasterItemResponse>());
    }
}