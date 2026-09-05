using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Extensions;
using SchoolERP.Application.Common.Interfaces;
using ClassGroupEntity = SchoolERP.Domain.Academic.Entities.ClassGroup;
using SchoolERP.Domain.Rbac.Entities;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Academic.Commands.ClassGroup.DeleteClassGroup;

public class DeleteClassGroupCommandHandler : IRequestHandler<DeleteClassGroupCommand, Result<bool>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentTenantService _tenantService;
    private readonly ICurrentUserService _currentUserService;

    public DeleteClassGroupCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentTenantService tenantService,
        ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _tenantService = tenantService;
        _currentUserService = currentUserService;
    }

    public async Task<Result<bool>> Handle(DeleteClassGroupCommand command, CancellationToken cancellationToken)
    {
        var branchId = _tenantService.GetBranchId();
        var tenantId = _tenantService.GetTenantId();

        var entityResult = await _dbContext.GetEntityAsync<ClassGroupEntity>(
            cg => cg.Id == command.Id && cg.BranchId == branchId && !cg.IsDeleted,
            "ClassGroup",
            command.Id.ToString(),
            cancellationToken);
        if (entityResult.IsFailure) return entityResult.Error;

        var classGroup = entityResult.Value;
        classGroup.IsDeleted = true;
        classGroup.DeletedAt = DateTime.UtcNow;
        classGroup.UpdatedAt = DateTime.UtcNow;

        var audit = new RbacAuditLog
        {
            TenantId = tenantId,
            PerformedBy = _currentUserService.GetUserId() ?? 0,
            Resource = "ClassGroup",
            Action = "delete",
            OldValues = System.Text.Json.JsonSerializer.Serialize(new { classGroup.Name, classGroup.Sequence, classGroup.Description, classGroup.IsActive }),
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Set<RbacAuditLog>().Add(audit);

        return Result.Success(true);
    }
}