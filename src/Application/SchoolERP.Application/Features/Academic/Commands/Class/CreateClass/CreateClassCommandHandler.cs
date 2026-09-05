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

namespace SchoolERP.Application.Features.Academic.Commands.Class.CreateClass;

public class CreateClassCommandHandler : IRequestHandler<CreateClassCommand, Result<ClassResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentTenantService _tenantService;
    private readonly ICurrentUserService _currentUserService;

    public CreateClassCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentTenantService tenantService,
        ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _tenantService = tenantService;
        _currentUserService = currentUserService;
    }

    public async Task<Result<ClassResponse>> Handle(CreateClassCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var branchId = _tenantService.GetBranchId();
        var tenantId = _tenantService.GetTenantId();

        // Check class group if provided
        if (request.ClassGroupId.HasValue)
        {
            var groupExists = await _dbContext.EnsureEntityExistsAsync<ClassGroupEntity>(request.ClassGroupId.Value, cancellationToken);
            if (groupExists != null) return groupExists;
        }

        // Uniqueness check for sequence
        var conflict = await _dbContext.EnsureUniqueAsync<ClassEntity>(
            c => c.BranchId == branchId && c.Sequence == request.Sequence && !c.IsDeleted,
            $"Class with sequence '{request.Sequence}' already exists.",
            cancellationToken);
        if (conflict != null) return conflict;

        var classEntity = request.Adapt<ClassEntity>();
        _dbContext.Set<ClassEntity>().Add(classEntity);

        // Audit
        var audit = new RbacAuditLog
        {
            TenantId = tenantId,
            PerformedBy = _currentUserService.GetUserId() ?? 0,
            Resource = "Class",
            Action = "create",
            NewValues = System.Text.Json.JsonSerializer.Serialize(new { request.Name, request.Sequence, request.ClassGroupId, request.IsActive }),
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Set<RbacAuditLog>().Add(audit);

        return Result.Success(classEntity.Adapt<ClassResponse>());
    }
}