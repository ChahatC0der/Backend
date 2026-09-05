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

namespace SchoolERP.Application.Features.Master.Commands.MasterItem.CreateMasterItem;

public class CreateMasterItemCommandHandler : IRequestHandler<CreateMasterItemCommand, Result<MasterItemResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentTenantService _tenantService;
    private readonly ICurrentUserService _currentUserService;

    public CreateMasterItemCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentTenantService tenantService,
        ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _tenantService = tenantService;
        _currentUserService = currentUserService;
    }

    public async Task<Result<MasterItemResponse>> Handle(CreateMasterItemCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var tenantId = _tenantService.GetTenantId();

        // Check category exists and is accessible
        var category = await _dbContext.Set<MasterCategoryEntity>()
            .FirstOrDefaultAsync(c => c.Id == request.CategoryId && !c.IsDeleted, cancellationToken);
        if (category == null)
            return Error.NotFound("MasterCategory", request.CategoryId.ToString());

        if (category.TenantId.HasValue && category.TenantId.Value != tenantId)
            return Error.NotFound("MasterCategory", request.CategoryId.ToString()); // or Forbidden

        // Uniqueness (CategoryId, Value)
        var conflict = await _dbContext.EnsureUniqueAsync<MasterItemEntity>(
            mi => mi.CategoryId == request.CategoryId && mi.Value == request.Value && !mi.IsDeleted,
            $"Master item '{request.Value}' already exists in this category.",
            cancellationToken);
        if (conflict != null) return conflict;

        var item = request.Adapt<MasterItemEntity>();
        _dbContext.Set<MasterItemEntity>().Add(item);

        // Audit
        var audit = new RbacAuditLog
        {
            TenantId = tenantId,
            PerformedBy = _currentUserService.GetUserId() ?? 0,
            Resource = "MasterItem",
            Action = "create",
            NewValues = System.Text.Json.JsonSerializer.Serialize(new { request.CategoryId, request.Value, request.Code, request.Description, request.Metadata, request.SortOrder, request.IsSystem, request.IsActive }),
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Set<RbacAuditLog>().Add(audit);

        return Result.Success(item.Adapt<MasterItemResponse>());
    }
}