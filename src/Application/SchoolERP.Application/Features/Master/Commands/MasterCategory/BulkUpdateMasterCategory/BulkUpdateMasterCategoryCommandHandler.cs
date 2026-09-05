using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Extensions;
using SchoolERP.Application.Common.Interfaces;
using MasterCategoryEntity = SchoolERP.Domain.Master.Entities.MasterCategory;
using SchoolERP.Domain.Rbac.Entities;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Master.Commands.MasterCategory.BulkUpdateMasterCategory;

public class BulkUpdateMasterCategoryCommandHandler : IRequestHandler<BulkUpdateMasterCategoryCommand, Result<bool>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentTenantService _tenantService;
    private readonly ICurrentUserService _currentUserService;

    public BulkUpdateMasterCategoryCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentTenantService tenantService,
        ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _tenantService = tenantService;
        _currentUserService = currentUserService;
    }

    public async Task<Result<bool>> Handle(BulkUpdateMasterCategoryCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var tenantId = _tenantService.GetTenantId();

        var moduleExists = await _dbContext.EnsureEntityExistsAsync<Module>(request.ModuleId, cancellationToken);
        if (moduleExists != null) return moduleExists;

        var entities = await _dbContext.Set<MasterCategoryEntity>()
            .Where(mc => mc.TenantId == tenantId && request.Ids.Contains(mc.Id) && !mc.IsDeleted)
            .ToListAsync(cancellationToken);

        if (entities.Count != request.Ids.Count)
            return Error.NotFound("MasterCategory", request.Ids.ToString());

        // Check uniqueness among selected (excluding them)
        var outsideDuplicate = await _dbContext.EnsureUniqueAsync<MasterCategoryEntity>(
            mc => mc.ModuleId == request.ModuleId && mc.TenantId == tenantId && mc.Key == request.Key && !request.Ids.Contains(mc.Id) && !mc.IsDeleted,
            $"Another master category with key '{request.Key}' already exists.",
            cancellationToken);
        if (outsideDuplicate != null) return outsideDuplicate;

        foreach (var mc in entities)
        {
            mc.ModuleId = request.ModuleId;
            mc.Key = request.Key;
            mc.Name = request.Name;
            mc.Description = request.Description;
            mc.IsSystem = request.IsSystem;
            mc.IsActive = request.IsActive;
            mc.UpdatedAt = DateTime.UtcNow;
        }

        var audit = new RbacAuditLog
        {
            TenantId = tenantId,
            PerformedBy = _currentUserService.GetUserId() ?? 0,
            Resource = "MasterCategory",
            Action = "bulk_update",
            NewValues = System.Text.Json.JsonSerializer.Serialize(new { request.Ids, request.ModuleId, request.Key, request.Name, request.Description, request.IsSystem, request.IsActive }),
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Set<RbacAuditLog>().Add(audit);

        return Result.Success(true);
    }
}