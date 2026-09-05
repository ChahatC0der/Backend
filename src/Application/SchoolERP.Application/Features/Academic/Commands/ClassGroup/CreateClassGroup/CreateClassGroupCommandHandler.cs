using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Extensions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Academic.DTOs;
using ClassGroupEntity = SchoolERP.Domain.Academic.Entities.ClassGroup;
using SchoolERP.Domain.Rbac.Entities;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Academic.Commands.ClassGroup.CreateClassGroup;

public class CreateClassGroupCommandHandler : IRequestHandler<CreateClassGroupCommand, Result<ClassGroupResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentTenantService _tenantService;
    private readonly ICurrentUserService _currentUserService;

    public CreateClassGroupCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentTenantService tenantService,
        ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _tenantService = tenantService;
        _currentUserService = currentUserService;
    }

    public async Task<Result<ClassGroupResponse>> Handle(CreateClassGroupCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var branchId = _tenantService.GetBranchId();
        var tenantId = _tenantService.GetTenantId();

        // Uniqueness check for sequence
        var conflict = await _dbContext.EnsureUniqueAsync<ClassGroupEntity>(
            cg => cg.BranchId == branchId && cg.Sequence == request.Sequence && !cg.IsDeleted,
            $"Class group with sequence '{request.Sequence}' already exists.",
            cancellationToken);
        if (conflict != null) return conflict;

        var classGroup = request.Adapt<ClassGroupEntity>();
        _dbContext.Set<ClassGroupEntity>().Add(classGroup);

        // Audit
        var audit = new RbacAuditLog
        {
            TenantId = tenantId,
            PerformedBy = _currentUserService.GetUserId() ?? 0,
            Resource = "ClassGroup",
            Action = "create",
            NewValues = System.Text.Json.JsonSerializer.Serialize(new { request.Name, request.Sequence, request.Description, request.IsActive }),
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Set<RbacAuditLog>().Add(audit);

        return Result.Success(classGroup.Adapt<ClassGroupResponse>());
    }
}