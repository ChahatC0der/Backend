using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Extensions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Academic.DTOs;
using ClassGroupEntity = SchoolERP.Domain.Academic.Entities.ClassGroup;
using ClassEntity = SchoolERP.Domain.Academic.Entities.Class;
using SchoolERP.Domain.Rbac.Entities;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Academic.Commands.Class.UpdateClass;

public class UpdateClassCommandHandler : IRequestHandler<UpdateClassCommand, Result<ClassResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentTenantService _tenantService;
    private readonly ICurrentUserService _currentUserService;

    public UpdateClassCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentTenantService tenantService,
        ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _tenantService = tenantService;
        _currentUserService = currentUserService;
    }

    public async Task<Result<ClassResponse>> Handle(UpdateClassCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var branchId = _tenantService.GetBranchId();
        var tenantId = _tenantService.GetTenantId();

        var entityResult = await _dbContext.GetEntityAsync<ClassEntity>(
            c => c.Id == request.Id && c.BranchId == branchId && !c.IsDeleted,
            "Class",
            request.Id.ToString(),
            cancellationToken);
        if (entityResult.IsFailure) return entityResult.Error;

        var classEntity = entityResult.Value;

        if (request.ClassGroupId.HasValue)
        {
            var groupExists = await _dbContext.EnsureEntityExistsAsync<ClassGroupEntity>(request.ClassGroupId.Value, cancellationToken);
            if (groupExists != null) return groupExists;
        }

        var conflict = await _dbContext.EnsureUniqueAsync<ClassEntity>(
            c => c.BranchId == branchId && c.Sequence == request.Sequence && c.Id != request.Id && !c.IsDeleted,
            $"Class with sequence '{request.Sequence}' already exists.",
            cancellationToken);
        if (conflict != null) return conflict;

        var oldValues = System.Text.Json.JsonSerializer.Serialize(new { classEntity.Name, classEntity.Sequence, classEntity.ClassGroupId, classEntity.IsActive });

        classEntity.Name = request.Name;
        classEntity.Sequence = request.Sequence;
        classEntity.ClassGroupId = request.ClassGroupId;
        classEntity.IsActive = request.IsActive;
        classEntity.UpdatedAt = DateTime.UtcNow;

        var audit = new RbacAuditLog
        {
            TenantId = tenantId,
            PerformedBy = _currentUserService.GetUserId() ?? 0,
            Resource = "Class",
            Action = "update",
            OldValues = oldValues,
            NewValues = System.Text.Json.JsonSerializer.Serialize(new { classEntity.Name, classEntity.Sequence, classEntity.ClassGroupId, classEntity.IsActive }),
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Set<RbacAuditLog>().Add(audit);

        return Result.Success(classEntity.Adapt<ClassResponse>());
    }
}