using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Extensions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Master.DTOs;
using MasterCategoryEntity = SchoolERP.Domain.Master.Entities.MasterCategory;
using SchoolERP.Domain.Rbac.Entities;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Master.Commands.MasterCategory.UpdateMasterCategory;

public class UpdateMasterCategoryCommandHandler : IRequestHandler<UpdateMasterCategoryCommand, Result<MasterCategoryResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentTenantService _tenantService;
    private readonly ICurrentUserService _currentUserService;

    public UpdateMasterCategoryCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentTenantService tenantService,
        ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _tenantService = tenantService;
        _currentUserService = currentUserService;
    }

    public async Task<Result<MasterCategoryResponse>> Handle(UpdateMasterCategoryCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var tenantId = _tenantService.GetTenantId();

        // Fetch using helper
        var entityResult = await _dbContext.GetEntityAsync< MasterCategoryEntity>(
            mc => mc.Id == request.Id && mc.TenantId == tenantId && !mc.IsDeleted,
            "MasterCategory",
            request.Id.ToString(),
            cancellationToken);
        if (entityResult.IsFailure) return entityResult.Error;

        var category = entityResult.Value;

        // Check module exists
        var moduleExists = await _dbContext.EnsureEntityExistsAsync<Module>(request.ModuleId, cancellationToken);
        if (moduleExists != null) return moduleExists;

        // Uniqueness check (excluding current)
        var conflict = await _dbContext.EnsureUniqueAsync<MasterCategoryEntity>(
            mc => mc.ModuleId == request.ModuleId && mc.TenantId == tenantId && mc.Key == request.Key && mc.Id != request.Id && !mc.IsDeleted,
            $"Master category with key '{request.Key}' already exists.",
            cancellationToken);
        if (conflict != null) return conflict;

        var oldValues = System.Text.Json.JsonSerializer.Serialize(new { category.ModuleId, category.Key, category.Name, category.Description, category.IsSystem, category.IsActive });

        category.ModuleId = request.ModuleId;
        category.Key = request.Key;
        category.Name = request.Name;
        category.Description = request.Description;
        category.IsSystem = request.IsSystem;
        category.IsActive = request.IsActive;
        category.UpdatedAt = DateTime.UtcNow;

        var audit = new RbacAuditLog
        {
            TenantId = tenantId,
            PerformedBy = _currentUserService.GetUserId() ?? 0,
            Resource = "MasterCategory",
            Action = "update",
            OldValues = oldValues,
            NewValues = System.Text.Json.JsonSerializer.Serialize(new { category.ModuleId, category.Key, category.Name, category.Description, category.IsSystem, category.IsActive }),
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Set<RbacAuditLog>().Add(audit);

        return Result.Success(category.Adapt<MasterCategoryResponse>());
    }
}