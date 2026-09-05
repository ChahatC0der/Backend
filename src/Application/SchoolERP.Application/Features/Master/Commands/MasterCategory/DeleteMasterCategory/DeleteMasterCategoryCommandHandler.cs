using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Extensions;
using SchoolERP.Application.Common.Interfaces;
using MasterCategoryEntity = SchoolERP.Domain.Master.Entities.MasterCategory;
using SchoolERP.Domain.Rbac.Entities;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Master.Commands.MasterCategory.DeleteMasterCategory;

public class DeleteMasterCategoryCommandHandler : IRequestHandler<DeleteMasterCategoryCommand, Result<bool>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentTenantService _tenantService;
    private readonly ICurrentUserService _currentUserService;

    public DeleteMasterCategoryCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentTenantService tenantService,
        ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _tenantService = tenantService;
        _currentUserService = currentUserService;
    }

    public async Task<Result<bool>> Handle(DeleteMasterCategoryCommand command, CancellationToken cancellationToken)
    {
        var tenantId = _tenantService.GetTenantId();

        var entityResult = await _dbContext.GetEntityAsync<MasterCategoryEntity>(
            mc => mc.Id == command.Id && mc.TenantId == tenantId && !mc.IsDeleted,
            "MasterCategory",
            command.Id.ToString(),
            cancellationToken);
        if (entityResult.IsFailure) return entityResult.Error;

        var category = entityResult.Value;

        // System categories cannot be deleted
        if (category.IsSystem)
            return Error.Conflict("System master category cannot be deleted.");

        category.IsDeleted = true;
        category.DeletedAt = DateTime.UtcNow;
        category.UpdatedAt = DateTime.UtcNow;

        var audit = new RbacAuditLog
        {
            TenantId = tenantId,
            PerformedBy = _currentUserService.GetUserId() ?? 0,
            Resource = "MasterCategory",
            Action = "delete",
            OldValues = System.Text.Json.JsonSerializer.Serialize(new { category.Id, category.Key, category.Name }),
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Set<RbacAuditLog>().Add(audit);

        return Result.Success(true);
    }
}