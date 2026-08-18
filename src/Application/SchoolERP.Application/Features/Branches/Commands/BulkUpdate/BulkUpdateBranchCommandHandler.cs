using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Domain.Shared.Results;
using SchoolERP.Domain.Tenants.Entities;

namespace SchoolERP.Application.Features.Branches.Commands.BulkUpdate;

public class BulkUpdateBranchCommandHandler : IRequestHandler<BulkUpdateBranchCommand, Result<int>>
{
    private readonly IApplicationDbContext _dbContext;

    public BulkUpdateBranchCommandHandler(IApplicationDbContext dbContext) => _dbContext = dbContext;

    public async Task<Result<int>> Handle(BulkUpdateBranchCommand request, CancellationToken cancellationToken)
    {
        var ids = request.Request.Ids.Distinct().ToList();

        var branches = await _dbContext.Set<Branch>()
            .Where(b => b.TenantId == request.TenantId && ids.Contains(b.Id) && !b.IsDeleted)
            .ToListAsync(cancellationToken);

        if (!branches.Any())
            return Error.NotFound("Branches", "No matching branches found.");

        // If setting default, reset other defaults (only if IsDefault is being updated)
        if (request.Request.IsDefault.HasValue && request.Request.IsDefault.Value)
        {
            var existingDefaults = await _dbContext.Set<Branch>()
                .Where(b => b.TenantId == request.TenantId && b.IsDefault && !ids.Contains(b.Id))
                .ToListAsync(cancellationToken);
            foreach (var d in existingDefaults) d.IsDefault = false;
        }

        foreach (var branch in branches)
        {
            if (!string.IsNullOrEmpty(request.Request.Status))
                branch.Status = request.Request.Status;
            if (request.Request.IsDefault.HasValue)
                branch.IsDefault = request.Request.IsDefault.Value;
            branch.UpdatedAt = DateTime.UtcNow;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(branches.Count, $"{branches.Count} branch(es) updated successfully.");
    }
}