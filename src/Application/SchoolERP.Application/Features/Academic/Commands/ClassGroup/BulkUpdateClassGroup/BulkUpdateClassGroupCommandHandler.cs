using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Extensions;
using SchoolERP.Application.Common.Interfaces;
using ClassGroupEntity = SchoolERP.Domain.Academic.Entities.ClassGroup;
using SchoolERP.Domain.Rbac.Entities;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Academic.Commands.ClassGroup.BulkUpdateClassGroup;

public class BulkUpdateClassGroupCommandHandler : IRequestHandler<BulkUpdateClassGroupCommand, Result<bool>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentTenantService _tenantService;
    private readonly ICurrentUserService _currentUserService;

    public BulkUpdateClassGroupCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentTenantService tenantService,
        ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _tenantService = tenantService;
        _currentUserService = currentUserService;
    }

    public async Task<Result<bool>> Handle(BulkUpdateClassGroupCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var branchId = _tenantService.GetBranchId();
        var tenantId = _tenantService.GetTenantId();

        var entities = await _dbContext.Set<ClassGroupEntity>()
            .Where(cg => cg.BranchId == branchId && request.Ids.Contains(cg.Id) && !cg.IsDeleted)
            .ToListAsync(cancellationToken);

        if (entities.Count != request.Ids.Count)
            return Error.NotFound("ClassGroup", request.Ids.ToString());

        // Check sequence uniqueness for all selected (if same sequence used by non-selected)
        var outsideDuplicate = await _dbContext.EnsureUniqueAsync<ClassGroupEntity>(
            cg => cg.BranchId == branchId && cg.Sequence == request.Sequence && !request.Ids.Contains(cg.Id) && !cg.IsDeleted,
            $"Another class group with sequence '{request.Sequence}' already exists.",
            cancellationToken);
        if (outsideDuplicate != null) return outsideDuplicate;

        foreach (var cg in entities)
        {
            cg.Name = request.Name;
            cg.Sequence = request.Sequence;
            cg.Description = request.Description;
            cg.IsActive = request.IsActive;
            cg.UpdatedAt = DateTime.UtcNow;
        }

        var audit = new RbacAuditLog
        {
            TenantId = tenantId,
            PerformedBy = _currentUserService.GetUserId() ?? 0,
            Resource = "ClassGroup",
            Action = "bulk_update",
            NewValues = System.Text.Json.JsonSerializer.Serialize(new { request.Ids, request.Name, request.Sequence, request.Description, request.IsActive }),
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Set<RbacAuditLog>().Add(audit);

        return Result.Success(true);
    }
}