using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using MasterCategoryEntity = SchoolERP.Domain.Master.Entities.MasterCategory;
using SchoolERP.Domain.Rbac.Entities;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Master.Commands.MasterCategory.RestoreMasterCategory;

public class RestoreMasterCategoryCommandHandler : IRequestHandler<RestoreMasterCategoryCommand, Result<bool>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentTenantService _tenantService;
    private readonly ICurrentUserService _currentUserService;

    public RestoreMasterCategoryCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentTenantService tenantService,
        ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _tenantService = tenantService;
        _currentUserService = currentUserService;
    }

    public async Task<Result<bool>> Handle(RestoreMasterCategoryCommand command, CancellationToken cancellationToken)
    {
        var tenantId = _tenantService.GetTenantId();

        var category = await _dbContext.Set<MasterCategoryEntity>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(mc => mc.Id == command.Id && mc.TenantId == tenantId && mc.IsDeleted, cancellationToken);

        if (category == null)
            return Error.NotFound("MasterCategory", command.Id.ToString());

        category.IsDeleted = false;
        category.DeletedAt = null;
        category.UpdatedAt = DateTime.UtcNow;

        var audit = new RbacAuditLog
        {
            TenantId = tenantId,
            PerformedBy = _currentUserService.GetUserId() ?? 0,
            Resource = "MasterCategory",
            Action = "restore",
            NewValues = System.Text.Json.JsonSerializer.Serialize(new { category.Id, category.Key, category.Name }),
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Set<RbacAuditLog>().Add(audit);

        return Result.Success(true);
    }
}