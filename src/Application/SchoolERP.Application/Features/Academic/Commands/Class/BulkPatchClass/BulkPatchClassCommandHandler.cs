using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Extensions;
using SchoolERP.Application.Common.Interfaces;
using ClassEntity = SchoolERP.Domain.Academic.Entities.Class;
using ClassGroupEntity = SchoolERP.Domain.Academic.Entities.ClassGroup;
using SchoolERP.Domain.Rbac.Entities;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Academic.Commands.Class.BulkPatchClass;

public class BulkPatchClassCommandHandler : IRequestHandler<BulkPatchClassCommand, Result<bool>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentTenantService _tenantService;
    private readonly ICurrentUserService _currentUserService;

    public BulkPatchClassCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentTenantService tenantService,
        ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _tenantService = tenantService;
        _currentUserService = currentUserService;
    }

    public async Task<Result<bool>> Handle(BulkPatchClassCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var branchId = _tenantService.GetBranchId();
        var tenantId = _tenantService.GetTenantId();

        if (request.ClassGroupId.HasValue)
        {
            var groupExists = await _dbContext.EnsureEntityExistsAsync<ClassGroupEntity>(request.ClassGroupId.Value, cancellationToken);
            if (groupExists != null) return groupExists;
        }

        var entities = await _dbContext.Set<ClassEntity>()
            .Where(c => c.BranchId == branchId && request.Ids.Contains(c.Id) && !c.IsDeleted)
            .ToListAsync(cancellationToken);

        if (entities.Count != request.Ids.Count)
            return Error.NotFound("Class", request.Ids.ToString());

        foreach (var c in entities)
        {
            request.Name.PatchIfProvided(value => c.Name = value);
            if (request.Sequence.HasValue)
            {
                var outsideDuplicate = await _dbContext.EnsureUniqueAsync<ClassEntity>(
                    x => x.BranchId == branchId && x.Sequence == request.Sequence.Value && x.Id != c.Id && !x.IsDeleted,
                    $"Another class with sequence '{request.Sequence.Value}' already exists.",
                    cancellationToken);
                if (outsideDuplicate != null) return outsideDuplicate;
                c.Sequence = request.Sequence.Value;
            }
            if (request.ClassGroupId.HasValue) c.ClassGroupId = request.ClassGroupId;
            if (request.IsActive.HasValue) c.IsActive = request.IsActive.Value;

            c.UpdatedAt = DateTime.UtcNow;
        }

        var audit = new RbacAuditLog
        {
            TenantId = tenantId,
            PerformedBy = _currentUserService.GetUserId() ?? 0,
            Resource = "Class",
            Action = "bulk_patch",
            NewValues = System.Text.Json.JsonSerializer.Serialize(new { request.Ids, request.Name, request.Sequence, request.ClassGroupId, request.IsActive }),
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Set<RbacAuditLog>().Add(audit);

        return Result.Success(true);
    }
}