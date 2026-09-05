using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Extensions;
using SchoolERP.Application.Common.Interfaces;
using ClassGroupEntity = SchoolERP.Domain.Academic.Entities.ClassGroup;
using SchoolERP.Domain.Rbac.Entities;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Academic.Commands.ClassGroup.BulkPatchClassGroup;

public class BulkPatchClassGroupCommandHandler : IRequestHandler<BulkPatchClassGroupCommand, Result<bool>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentTenantService _tenantService;
    private readonly ICurrentUserService _currentUserService;

    public BulkPatchClassGroupCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentTenantService tenantService,
        ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _tenantService = tenantService;
        _currentUserService = currentUserService;
    }

    public async Task<Result<bool>> Handle(BulkPatchClassGroupCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var branchId = _tenantService.GetBranchId();
        var tenantId = _tenantService.GetTenantId();

        var entities = await _dbContext.Set<ClassGroupEntity>()
            .Where(cg => cg.BranchId == branchId && request.Ids.Contains(cg.Id) && !cg.IsDeleted)
            .ToListAsync(cancellationToken);

        if (entities.Count != request.Ids.Count)
            return Error.NotFound("ClassGroup", request.Ids.ToString());

        foreach (var cg in entities)
        {
            request.Name.PatchIfProvided(value => cg.Name = value);
            if (request.Sequence.HasValue)
            {
                var outsideDuplicate = await _dbContext.EnsureUniqueAsync<ClassGroupEntity>(
                    x => x.BranchId == branchId && x.Sequence == request.Sequence.Value && x.Id != cg.Id && !x.IsDeleted,
                    $"Another class group with sequence '{request.Sequence.Value}' already exists.",
                    cancellationToken);
                if (outsideDuplicate != null) return outsideDuplicate;
                cg.Sequence = request.Sequence.Value;
            }
            if (request.Description != null) cg.Description = request.Description;
            if (request.IsActive.HasValue) cg.IsActive = request.IsActive.Value;

            cg.UpdatedAt = DateTime.UtcNow;
        }

        var audit = new RbacAuditLog
        {
            TenantId = tenantId,
            PerformedBy = _currentUserService.GetUserId() ?? 0,
            Resource = "ClassGroup",
            Action = "bulk_patch",
            NewValues = System.Text.Json.JsonSerializer.Serialize(new { request.Ids, request.Name, request.Sequence, request.Description, request.IsActive }),
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Set<RbacAuditLog>().Add(audit);

        return Result.Success(true);
    }
}