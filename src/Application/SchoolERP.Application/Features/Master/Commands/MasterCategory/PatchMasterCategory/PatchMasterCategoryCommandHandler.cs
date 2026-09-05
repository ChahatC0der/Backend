using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Extensions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Master.DTOs;
using MasterCategoryEntity = SchoolERP.Domain.Master.Entities.MasterCategory;
using SchoolERP.Domain.Rbac.Entities;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Master.Commands.MasterCategory.PatchMasterCategory;

public class PatchMasterCategoryCommandHandler : IRequestHandler<PatchMasterCategoryCommand, Result<MasterCategoryResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentTenantService _tenantService;
    private readonly ICurrentUserService _currentUserService;

    public PatchMasterCategoryCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentTenantService tenantService,
        ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _tenantService = tenantService;
        _currentUserService = currentUserService;
    }

    public async Task<Result<MasterCategoryResponse>> Handle(PatchMasterCategoryCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var tenantId = _tenantService.GetTenantId();

        var entityResult = await _dbContext.GetEntityAsync<MasterCategoryEntity>(
            mc => mc.Id == request.Id && mc.TenantId == tenantId && !mc.IsDeleted,
            "MasterCategory",
            request.Id.ToString(),
            cancellationToken);
        if (entityResult.IsFailure) return entityResult.Error;

        var category = entityResult.Value;
        var oldValues = System.Text.Json.JsonSerializer.Serialize(new { category.ModuleId, category.Key, category.Name, category.Description, category.IsSystem, category.IsActive });

        if (request.ModuleId.HasValue)
        {
            var moduleExists = await _dbContext.EnsureEntityExistsAsync<Module>(request.ModuleId.Value, cancellationToken);
            if (moduleExists != null) return moduleExists;
            category.ModuleId = request.ModuleId.Value;
        }

        request.Key.PatchIfProvided(value => category.Key = value);
        request.Name.PatchIfProvided(value => category.Name = value);
        if (request.Description != null) category.Description = request.Description;
        if (request.IsSystem.HasValue) category.IsSystem = request.IsSystem.Value;
        if (request.IsActive.HasValue) category.IsActive = request.IsActive.Value;

        // Check uniqueness if key or module changed
        if (!string.IsNullOrWhiteSpace(request.Key) || request.ModuleId.HasValue)
        {
            var conflict = await _dbContext.EnsureUniqueAsync<MasterCategoryEntity>(
                mc => mc.ModuleId == category.ModuleId && mc.TenantId == tenantId && mc.Key == category.Key && mc.Id != category.Id && !mc.IsDeleted,
                $"Master category with key '{category.Key}' already exists.",
                cancellationToken);
            if (conflict != null) return conflict;
        }

        category.UpdatedAt = DateTime.UtcNow;

        var audit = new RbacAuditLog
        {
            TenantId = tenantId,
            PerformedBy = _currentUserService.GetUserId() ?? 0,
            Resource = "MasterCategory",
            Action = "patch",
            OldValues = oldValues,
            NewValues = System.Text.Json.JsonSerializer.Serialize(new { category.ModuleId, category.Key, category.Name, category.Description, category.IsSystem, category.IsActive }),
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Set<RbacAuditLog>().Add(audit);

        return Result.Success(category.Adapt<MasterCategoryResponse>());
    }
}