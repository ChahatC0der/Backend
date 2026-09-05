using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Extensions;
using SchoolERP.Application.Common.Interfaces;
using ClassEntity = SchoolERP.Domain.Academic.Entities.Class;
using ClassGroupEntity = SchoolERP.Domain.Academic.Entities.ClassGroup;
using SchoolERP.Domain.Rbac.Entities;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Academic.Commands.Class.BulkUpdateClass;

public class BulkUpdateClassCommandHandler : IRequestHandler<BulkUpdateClassCommand, Result<bool>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentTenantService _tenantService;
    private readonly ICurrentUserService _currentUserService;

    public BulkUpdateClassCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentTenantService tenantService,
        ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _tenantService = tenantService;
        _currentUserService = currentUserService;
    }

    public async Task<Result<bool>> Handle(BulkUpdateClassCommand command, CancellationToken cancellationToken)
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

        // Ensure sequence uniqueness among all classes outside selected
        var outsideDuplicate = await _dbContext.EnsureUniqueAsync<ClassEntity>(
            c => c.BranchId == branchId && c.Sequence == request.Sequence && !request.Ids.Contains(c.Id) && !c.IsDeleted,
            $"Another class with sequence '{request.Sequence}' already exists.",
            cancellationToken);
        if (outsideDuplicate != null) return outsideDuplicate;

        foreach (var c in entities)
        {
            c.Name = request.Name;
            c.Sequence = request.Sequence;
            c.ClassGroupId = request.ClassGroupId;
            c.IsActive = request.IsActive;
            c.UpdatedAt = DateTime.UtcNow;
        }

        var audit = new RbacAuditLog
        {
            TenantId = tenantId,
            PerformedBy = _currentUserService.GetUserId() ?? 0,
            Resource = "Class",
            Action = "bulk_update",
            NewValues = System.Text.Json.JsonSerializer.Serialize(new { request.Ids, request.Name, request.Sequence, request.ClassGroupId, request.IsActive }),
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Set<RbacAuditLog>().Add(audit);

        return Result.Success(true);
    }
}