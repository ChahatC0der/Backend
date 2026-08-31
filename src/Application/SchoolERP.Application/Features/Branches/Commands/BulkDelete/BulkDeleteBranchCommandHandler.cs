using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Domain.Shared.Results;
using SchoolERP.Domain.Tenants.Entities;

namespace SchoolERP.Application.Features.Branches.Commands.BulkDelete;

public class BulkDeleteBranchCommandHandler : IRequestHandler<BulkDeleteBranchCommand, Result<int>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentTenantService _currentTenant;

    public BulkDeleteBranchCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentTenantService currentTenant)
    {
        _dbContext = dbContext;
        _currentTenant = currentTenant;
    }

    public async Task<Result<int>> Handle(
        BulkDeleteBranchCommand request,
        CancellationToken cancellationToken)
    {
        // 1️⃣ GET CURRENT TENANT FROM FINBUCKLE
        var tenantId = _currentTenant.GetTenantId();

        if (tenantId == Guid.Empty)
            return Error.NotFound("Tenant", "Tenant context not resolved.");

        // 2️⃣ FETCH BRANCHES
        var branches = await _dbContext.Set<Branch>()
            .Where(b =>
                b.TenantId == tenantId &&
                request.Request.Ids.Contains(b.Id) &&
                !b.IsDeleted)
            .ToListAsync(cancellationToken);

        if (!branches.Any())
            return Error.NotFound("Branches", "No matching branches found.");

        // 3️⃣ SOFT DELETE
        foreach (var branch in branches)
        {
            branch.IsDeleted = true;
            branch.DeletedAt = DateTime.UtcNow;
            branch.Status = "closed";
            // UpdatedAt auto-set by SaveChangesAsync
        }

        // 4️⃣ SAVE
        await _dbContext.SaveChangesAsync(cancellationToken);

        // 5️⃣ RETURN
        return Result.Success(
            branches.Count,
            $"{branches.Count} branch(es) deleted successfully.");
    }
}