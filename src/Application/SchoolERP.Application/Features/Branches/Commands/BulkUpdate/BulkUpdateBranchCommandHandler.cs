using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Domain.Shared.Results;
using SchoolERP.Domain.Tenants.Entities;
using System.Linq.Expressions;

namespace SchoolERP.Application.Features.Branches.Commands.BulkUpdate;

public class BulkUpdateBranchCommandHandler : IRequestHandler<BulkUpdateBranchCommand, Result<int>>
{
    private readonly IApplicationDbContext _dbContext;

    public BulkUpdateBranchCommandHandler(IApplicationDbContext dbContext)
        => _dbContext = dbContext;

    public async Task<Result<int>> Handle(BulkUpdateBranchCommand request, CancellationToken cancellationToken)
    {
        var ids = request.Request.Ids.Distinct().ToList();

        // 1️⃣ FETCH BRANCHES
        var branches = await _dbContext.Set<Branch>()
            .Where(b => b.TenantId == request.TenantId && ids.Contains(b.Id) && !b.IsDeleted)
            .ToListAsync(cancellationToken);

        if (!branches.Any())
            return Error.NotFound("Branches", "No matching branches found.");

        // 2️⃣ 🔥 UNIQUENESS CHECKS (EXCLUDE BRANCHES BEING UPDATED)
        var checks = new List<(Expression<Func<Branch, bool>> Predicate, string Message)>();

        // Only if Code is being updated (BulkUpdate only has Status + IsDefault, but keeping for extensibility)
        // Since BulkUpdateBranchRequest only has Status and IsDefault, we skip code checks.

        // 3️⃣ RESET OTHER DEFAULTS (IF SETTING DEFAULT)
        if (request.Request.IsDefault.HasValue && request.Request.IsDefault.Value)
        {
            var existingDefaults = await _dbContext.Set<Branch>()
                .Where(b => b.TenantId == request.TenantId && b.IsDefault && !ids.Contains(b.Id))
                .ToListAsync(cancellationToken);

            foreach (var d in existingDefaults)
                d.IsDefault = false;
        }

        // 4️⃣ APPLY UPDATES
        foreach (var branch in branches)
        {
            if (!string.IsNullOrEmpty(request.Request.Status))
                branch.Status = request.Request.Status;

            if (request.Request.IsDefault.HasValue)
                branch.IsDefault = request.Request.IsDefault.Value;

            // 🔥 UpdatedAt auto-set by SaveChangesAsync override
        }

        // 5️⃣ SAVE (AUTO AUDIT)
        await _dbContext.SaveChangesAsync(cancellationToken);

        // 6️⃣ RETURN
        return Result.Success(branches.Count, $"{branches.Count} branch(es) updated successfully.");
    }
}