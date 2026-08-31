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
    private readonly ICurrentTenantService _currentTenant;

    public BulkUpdateBranchCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentTenantService currentTenant)
    {
        _dbContext = dbContext;
        _currentTenant = currentTenant;
    }

    public async Task<Result<int>> Handle(
        BulkUpdateBranchCommand request,
        CancellationToken cancellationToken)
    {
        var tenantId = _currentTenant.GetTenantId();

        if (tenantId == Guid.Empty)
            return Error.NotFound("Tenant", "Tenant context not resolved.");

        var ids = request.Request.Ids.Distinct().ToList();

        // 1️⃣ FETCH BRANCHES
        var branches = await _dbContext.Set<Branch>()
            .Where(b => b.TenantId == tenantId && ids.Contains(b.Id) && !b.IsDeleted)
            .ToListAsync(cancellationToken);

        if (!branches.Any())
            return Error.NotFound("Branches", "No matching branches found.");

        // 2️⃣ 🔥 UNIQUENESS CHECKS
        var checks = new List<(Expression<Func<Branch, bool>> Predicate, string Message)>();

        // BulkUpdateBranchRequest only has Status + IsDefault,
        // so no Code uniqueness check is required.

        // 3️⃣ RESET OTHER DEFAULTS (IF SETTING DEFAULT)
        if (request.Request.IsDefault.HasValue && request.Request.IsDefault.Value)
        {
            var existingDefaults = await _dbContext.Set<Branch>()
                .Where(b =>
                    b.TenantId == tenantId &&
                    b.IsDefault &&
                    !ids.Contains(b.Id))
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

            // UpdatedAt auto-set by SaveChangesAsync override
        }

        // 5️⃣ SAVE (AUTO AUDIT)
        await _dbContext.SaveChangesAsync(cancellationToken);

        // 6️⃣ RETURN
        return Result.Success(
            branches.Count,
            $"{branches.Count} branch(es) updated successfully.");
    }
}