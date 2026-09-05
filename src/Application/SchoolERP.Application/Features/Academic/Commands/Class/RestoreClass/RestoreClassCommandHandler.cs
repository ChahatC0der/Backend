using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using ClassEntity = SchoolERP.Domain.Academic.Entities.Class;
using SchoolERP.Domain.Rbac.Entities;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Academic.Commands.Class.RestoreClass;

public class RestoreClassCommandHandler : IRequestHandler<RestoreClassCommand, Result<bool>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentTenantService _tenantService;
    private readonly ICurrentUserService _currentUserService;

    public RestoreClassCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentTenantService tenantService,
        ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _tenantService = tenantService;
        _currentUserService = currentUserService;
    }

    public async Task<Result<bool>> Handle(RestoreClassCommand command, CancellationToken cancellationToken)
    {
        var branchId = _tenantService.GetBranchId();
        var tenantId = _tenantService.GetTenantId();

        var classEntity = await _dbContext.Set<ClassEntity>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.Id == command.Id && c.BranchId == branchId && c.IsDeleted, cancellationToken);

        if (classEntity == null)
            return Error.NotFound("Class", command.Id.ToString());

        classEntity.IsDeleted = false;
        classEntity.DeletedAt = null;
        classEntity.UpdatedAt = DateTime.UtcNow;

        var audit = new RbacAuditLog
        {
            TenantId = tenantId,
            PerformedBy = _currentUserService.GetUserId() ?? 0,
            Resource = "Class",
            Action = "restore",
            NewValues = System.Text.Json.JsonSerializer.Serialize(new { classEntity.Id, classEntity.Name }),
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Set<RbacAuditLog>().Add(audit);

        return Result.Success(true);
    }
}