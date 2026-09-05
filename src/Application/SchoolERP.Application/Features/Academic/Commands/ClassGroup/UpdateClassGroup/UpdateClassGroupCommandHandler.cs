using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Extensions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Academic.DTOs;
using ClassGroupEntity = SchoolERP.Domain.Academic.Entities.ClassGroup;
using SchoolERP.Domain.Rbac.Entities;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Academic.Commands.ClassGroup.UpdateClassGroup;

public class UpdateClassGroupCommandHandler : IRequestHandler<UpdateClassGroupCommand, Result<ClassGroupResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentTenantService _tenantService;
    private readonly ICurrentUserService _currentUserService;

    public UpdateClassGroupCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentTenantService tenantService,
        ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _tenantService = tenantService;
        _currentUserService = currentUserService;
    }

    public async Task<Result<ClassGroupResponse>> Handle(UpdateClassGroupCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var branchId = _tenantService.GetBranchId();
        var tenantId = _tenantService.GetTenantId();

        var entityResult = await _dbContext.GetEntityAsync<ClassGroupEntity>(
            cg => cg.Id == request.Id && cg.BranchId == branchId && !cg.IsDeleted,
            "ClassGroup",
            request.Id.ToString(),
            cancellationToken);
        if (entityResult.IsFailure) return entityResult.Error;

        var classGroup = entityResult.Value;

        var conflict = await _dbContext.EnsureUniqueAsync<ClassGroupEntity>(
            cg => cg.BranchId == branchId && cg.Sequence == request.Sequence && cg.Id != request.Id && !cg.IsDeleted,
            $"Class group with sequence '{request.Sequence}' already exists.",
            cancellationToken);
        if (conflict != null) return conflict;

        var oldValues = System.Text.Json.JsonSerializer.Serialize(new { classGroup.Name, classGroup.Sequence, classGroup.Description, classGroup.IsActive });

        classGroup.Name = request.Name;
        classGroup.Sequence = request.Sequence;
        classGroup.Description = request.Description;
        classGroup.IsActive = request.IsActive;
        classGroup.UpdatedAt = DateTime.UtcNow;

        var audit = new RbacAuditLog
        {
            TenantId = tenantId,
            PerformedBy = _currentUserService.GetUserId() ?? 0,
            Resource = "ClassGroup",
            Action = "update",
            OldValues = oldValues,
            NewValues = System.Text.Json.JsonSerializer.Serialize(new { classGroup.Name, classGroup.Sequence, classGroup.Description, classGroup.IsActive }),
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Set<RbacAuditLog>().Add(audit);

        return Result.Success(classGroup.Adapt<ClassGroupResponse>());
    }
}