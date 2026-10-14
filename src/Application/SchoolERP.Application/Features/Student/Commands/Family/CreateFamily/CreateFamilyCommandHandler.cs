using Mapster;
using MediatR;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Student.DTOs;
using SchoolERP.Domain.Rbac.Entities;
using SchoolERP.Domain.Shared.Results;
using FamilyEntity = SchoolERP.Domain.Student.Entities.Family;

namespace SchoolERP.Application.Features.Student.Commands.Family.CreateFamily;

public class CreateFamilyCommandHandler : IRequestHandler<CreateFamilyCommand, Result<FamilyResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentBranchService _branchService;
    private readonly ICurrentTenantService _tenantService;
    private readonly ICurrentUserService _currentUserService;

    public CreateFamilyCommandHandler(
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

    public async Task<Result<FamilyResponse>> Handle(CreateFamilyCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var branchId = _branchService.GetBranchId();
        var tenantId = _tenantService.GetTenantId();

        if (branchId == null) return Error.Conflict("Please Choose a Branch");

        var family = request.Adapt<FamilyEntity>();
        family.TenantId = tenantId;

        _dbContext.Set<FamilyEntity>().Add(family);

        var userId = _currentUserService.GetUserId() ?? 0;
        _dbContext.Set<RbacAuditLog>().Add(new RbacAuditLog
        {
            TenantId = tenantId,
            PerformedBy = userId,
            Resource = "Family",
            Action = "create",
            NewValues = System.Text.Json.JsonSerializer.Serialize(new { family.HeadName, family.HeadMobile }),
            CreatedAt = DateTime.UtcNow
        });

        return Result.Success(family.Adapt<FamilyResponse>());
    }
}
