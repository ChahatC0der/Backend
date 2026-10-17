using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Extensions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Student.DTOs;
using SchoolERP.Domain.Rbac.Entities;
using SchoolERP.Domain.Shared.Results;
using FamilyEntity = SchoolERP.Domain.Student.Entities.Family;

namespace SchoolERP.Application.Features.Student.Commands.Family.UpdateFamily;

public class UpdateFamilyCommandHandler : IRequestHandler<UpdateFamilyCommand, Result<FamilyResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentBranchService _branchService;
    private readonly ICurrentTenantService _tenantService;
    private readonly ICurrentUserService _currentUserService;

    public UpdateFamilyCommandHandler(
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

    public async Task<Result<FamilyResponse>> Handle(UpdateFamilyCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var branchId = _branchService.GetBranchId();
        var tenantId = _tenantService.GetTenantId();

        if (branchId == null) return Error.Conflict("Please Choose a Branch");

        var entityResult = await _dbContext.GetEntityAsync<FamilyEntity>(
            f => f.Id == request.Id && f.BranchId == branchId && !f.IsDeleted,
            "Family", request.Id.ToString(), cancellationToken);
        if (entityResult.IsFailure) return entityResult.Error;

        var family = entityResult.Value;

        var oldValues = System.Text.Json.JsonSerializer.Serialize(new { family.HeadName, family.HeadMobile, family.HeadEmail });

        family.HeadName = request.HeadName;
        family.HeadMobile = request.HeadMobile;
        family.HeadEmail = request.HeadEmail;
        family.UpdatedAt = DateTime.UtcNow;

        var userId = _currentUserService.GetUserId() ?? 0;
        //_dbContext.Set<RbacAuditLog>().Add(new RbacAuditLog
        //{
        //    TenantId = tenantId,
        //    PerformedBy = userId,
        //    Resource = "Family",
        //    Action = "update",
        //    OldValues = oldValues,
        //    NewValues = System.Text.Json.JsonSerializer.Serialize(new { family.HeadName, family.HeadMobile, family.HeadEmail }),
        //    CreatedAt = DateTime.UtcNow
        //});

        return Result.Success(family.Adapt<FamilyResponse>());
    }
}
