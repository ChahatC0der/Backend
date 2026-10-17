using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Domain.Rbac.Entities;
using SchoolERP.Domain.Shared.Results;
using FamilyEntity = SchoolERP.Domain.Student.Entities.Family;

namespace SchoolERP.Application.Features.Student.Commands.Family.RestoreFamily;

public class RestoreFamilyCommandHandler : IRequestHandler<RestoreFamilyCommand, Result<bool>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentBranchService _branchService;
    private readonly ICurrentTenantService _tenantService;
    private readonly ICurrentUserService _currentUserService;

    public RestoreFamilyCommandHandler(
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

    public async Task<Result<bool>> Handle(RestoreFamilyCommand command, CancellationToken cancellationToken)
    {
        var branchId = _branchService.GetBranchId();
        var tenantId = _tenantService.GetTenantId();

        if (branchId == null) return Error.Conflict("Please Choose a Branch");

        var family = await _dbContext.Set<FamilyEntity>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(f => f.Id == command.Id && f.BranchId == branchId && f.IsDeleted, cancellationToken);

        if (family == null)
            return Error.NotFound("Family", command.Id.ToString());

        family.IsDeleted = false;
        family.DeletedAt = null;
        family.UpdatedAt = DateTime.UtcNow;

        var userId = _currentUserService.GetUserId() ?? 0;
        //_dbContext.Set<RbacAuditLog>().Add(new RbacAuditLog
        //{
        //    TenantId = tenantId,
        //    PerformedBy = userId,
        //    Resource = "Family",
        //    Action = "restore",
        //    NewValues = System.Text.Json.JsonSerializer.Serialize(new { family.Id, family.HeadName }),
        //    CreatedAt = DateTime.UtcNow
        //});

        return Result.Success(true);
    }
}
