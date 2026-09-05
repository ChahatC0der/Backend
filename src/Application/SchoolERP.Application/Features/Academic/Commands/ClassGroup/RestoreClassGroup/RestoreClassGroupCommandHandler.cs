using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using ClassGroupEntity = SchoolERP.Domain.Academic.Entities.ClassGroup;
using SchoolERP.Domain.Rbac.Entities;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Academic.Commands.ClassGroup.RestoreClassGroup;

public class RestoreClassGroupCommandHandler : IRequestHandler<RestoreClassGroupCommand, Result<bool>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentTenantService _tenantService;
    private readonly ICurrentUserService _currentUserService;

    public RestoreClassGroupCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentTenantService tenantService,
        ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _tenantService = tenantService;
        _currentUserService = currentUserService;
    }

    public async Task<Result<bool>> Handle(RestoreClassGroupCommand command, CancellationToken cancellationToken)
    {
        var branchId = _tenantService.GetBranchId();
        var tenantId = _tenantService.GetTenantId();

        var classGroup = await _dbContext.Set<ClassGroupEntity>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(cg => cg.Id == command.Id && cg.BranchId == branchId && cg.IsDeleted, cancellationToken);

        if (classGroup == null)
            return Error.NotFound("ClassGroup", command.Id.ToString());

        classGroup.IsDeleted = false;
        classGroup.DeletedAt = null;
        classGroup.UpdatedAt = DateTime.UtcNow;

        var audit = new RbacAuditLog
        {
            TenantId = tenantId,
            PerformedBy = _currentUserService.GetUserId() ?? 0,
            Resource = "ClassGroup",
            Action = "restore",
            NewValues = System.Text.Json.JsonSerializer.Serialize(new { classGroup.Id, classGroup.Name }),
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Set<RbacAuditLog>().Add(audit);

        return Result.Success(true);
    }
}