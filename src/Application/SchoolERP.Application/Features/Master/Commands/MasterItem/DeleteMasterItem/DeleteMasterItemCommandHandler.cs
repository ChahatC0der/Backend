using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Extensions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Domain.Master.Entities;
using SchoolERP.Domain.Rbac.Entities;
using SchoolERP.Domain.Shared.Results;
using MasterItemEntity = SchoolERP.Domain.Master.Entities.MasterItem;

namespace SchoolERP.Application.Features.Master.Commands.MasterItem.DeleteMasterItem;

public class DeleteMasterItemCommandHandler : IRequestHandler<DeleteMasterItemCommand, Result<bool>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentTenantService _tenantService;
    private readonly ICurrentUserService _currentUserService;

    public DeleteMasterItemCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentTenantService tenantService,
        ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _tenantService = tenantService;
        _currentUserService = currentUserService;
    }

    public async Task<Result<bool>> Handle(DeleteMasterItemCommand command, CancellationToken cancellationToken)
    {
        var tenantId = _tenantService.GetTenantId();

        var item = await _dbContext.Set<MasterItemEntity>()
            .Include(mi => mi.Category)
            .FirstOrDefaultAsync(mi => mi.Id == command.Id && !mi.IsDeleted, cancellationToken);
        if (item == null)
            return Error.NotFound("MasterItem", command.Id.ToString());

        // Ensure category accessible
        if (item.Category.TenantId.HasValue && item.Category.TenantId.Value != tenantId)
            return Error.NotFound("MasterItem", command.Id.ToString());

        if (item.IsSystem)
            return Error.Conflict("System master item cannot be deleted.");

        item.IsDeleted = true;
        item.DeletedAt = DateTime.UtcNow;
        item.UpdatedAt = DateTime.UtcNow;

        var audit = new RbacAuditLog
        {
            TenantId = tenantId,
            PerformedBy = _currentUserService.GetUserId() ?? 0,
            Resource = "MasterItem",
            Action = "delete",
            OldValues = System.Text.Json.JsonSerializer.Serialize(new { item.Id, item.Value, item.Code }),
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Set<RbacAuditLog>().Add(audit);

        return Result.Success(true);
    }
}