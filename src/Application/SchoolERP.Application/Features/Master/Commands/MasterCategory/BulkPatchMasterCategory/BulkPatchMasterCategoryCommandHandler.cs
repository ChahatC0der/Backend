using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Extensions;
using SchoolERP.Application.Common.Interfaces;
using MasterCategoryEntity = SchoolERP.Domain.Master.Entities.MasterCategory;
using SchoolERP.Domain.Rbac.Entities;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Master.Commands.MasterCategory.BulkPatchMasterCategory;

public class BulkPatchMasterCategoryCommandHandler : IRequestHandler<BulkPatchMasterCategoryCommand, Result<bool>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentTenantService _tenantService;
    private readonly ICurrentUserService _currentUserService;

    public BulkPatchMasterCategoryCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentTenantService tenantService,
        ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _tenantService = tenantService;
        _currentUserService = currentUserService;
    }

    public async Task<Result<bool>> Handle(BulkPatchMasterCategoryCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var tenantId = _tenantService.GetTenantId();

        var entities = await _dbContext.Set<MasterCategoryEntity>()
            .Where(mc => mc.TenantId == tenantId && request.Ids.Contains(mc.Id) && !mc.IsDeleted)
            .ToListAsync(cancellationToken);

        if (entities.Count != request.Ids.Count)
            return Error.NotFound("MasterCategory", request.Ids.ToString());

        foreach (var mc in entities)
        {
            if (request.ModuleId.HasValue)
            {
                var moduleExists = await _dbContext.EnsureEntityExistsAsync<Module>(request.ModuleId.Value, cancellationToken);
                if (moduleExists != null) return moduleExists;
                mc.ModuleId = request.ModuleId.Value;
            }

            request.Key.PatchIfProvided(value => mc.Key = value);
            request.Name.PatchIfProvided(value => mc.Name = value);
            if (request.Description != null) mc.Description = request.Description;
            if (request.IsSystem.HasValue) mc.IsSystem = request.IsSystem.Value;
            if (request.IsActive.HasValue) mc.IsActive = request.IsActive.Value;

            // Check uniqueness for each entity if key/module changed
            if (!string.IsNullOrWhiteSpace(request.Key) || request.ModuleId.HasValue)
            {
                var conflict = await _dbContext.EnsureUniqueAsync<MasterCategoryEntity>(
                    x => x.ModuleId == mc.ModuleId && x.TenantId == tenantId && x.Key == mc.Key && x.Id != mc.Id && !x.IsDeleted,
                    $"Another master category with key '{mc.Key}' already exists.",
                    cancellationToken);
                if (conflict != null) return conflict;
            }

            mc.UpdatedAt = DateTime.UtcNow;
        }

        var audit = new RbacAuditLog
        {
            TenantId = tenantId,
            PerformedBy = _currentUserService.GetUserId() ?? 0,
            Resource = "MasterCategory",
            Action = "bulk_patch",
            NewValues = System.Text.Json.JsonSerializer.Serialize(new { request.Ids, request.ModuleId, request.Key, request.Name, request.Description, request.IsSystem, request.IsActive }),
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Set<RbacAuditLog>().Add(audit);

        return Result.Success(true);
    }
}