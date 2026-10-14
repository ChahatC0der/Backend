using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Extensions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Domain.Rbac.Entities;
using SchoolERP.Domain.Shared.Results;
using FamilyEntity = SchoolERP.Domain.Student.Entities.Family;
using StudentEntity = SchoolERP.Domain.Student.Entities.Student;

namespace SchoolERP.Application.Features.Student.Commands.Family.DeleteFamily;

public class DeleteFamilyCommandHandler : IRequestHandler<DeleteFamilyCommand, Result<bool>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentBranchService _branchService;
    private readonly ICurrentTenantService _tenantService;
    private readonly ICurrentUserService _currentUserService;

    public DeleteFamilyCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentBranchService branchService,
        ICurrentTenantService tenantService,
        ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _branchService = branchService;
        _tenantService = tenantService;
        _currentUserService = currentUserService;
    }

    public async Task<Result<bool>> Handle(DeleteFamilyCommand command, CancellationToken cancellationToken)
    {
        var branchId = _branchService.GetBranchId();
        var tenantId = _tenantService.GetTenantId();

        if (branchId == null) return Error.Conflict("Please Choose a Branch");

        var entityResult = await _dbContext.GetEntityAsync<FamilyEntity>(
            f => f.Id == command.Id && f.BranchId == branchId && !f.IsDeleted,
            "Family", command.Id.ToString(), cancellationToken);
        if (entityResult.IsFailure) return entityResult.Error;

        var family = entityResult.Value;

        // Block delete if students are linked
        var linkedCount = await _dbContext.Set<StudentEntity>()
            .CountAsync(s => s.FamilyId == family.Id && !s.IsDeleted, cancellationToken);

        if (linkedCount > 0)
            return Error.Conflict($"{linkedCount} student(s) are linked to this family. Unlink them first.");

        family.IsDeleted = true;
        family.DeletedAt = DateTime.UtcNow;
        family.UpdatedAt = DateTime.UtcNow;

        var userId = _currentUserService.GetUserId() ?? 0;
        _dbContext.Set<RbacAuditLog>().Add(new RbacAuditLog
        {
            TenantId = tenantId,
            PerformedBy = userId,
            Resource = "Family",
            Action = "delete",
            OldValues = System.Text.Json.JsonSerializer.Serialize(new { family.HeadName, family.HeadMobile }),
            CreatedAt = DateTime.UtcNow
        });

        return Result.Success(true);
    }
}
