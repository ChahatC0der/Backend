using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Extensions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Domain.Shared.Results;
using SchoolERP.Domain.Tenants.Entities;
using System.Linq.Expressions;

namespace SchoolERP.Application.Features.Branches.Commands.RestoreBranch;

public class RestoreBranchCommandHandler : IRequestHandler<RestoreBranchCommand, Result<bool>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentTenantService _currentTenant;

    public RestoreBranchCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentTenantService currentTenant)
    {
        _dbContext = dbContext;
        _currentTenant = currentTenant;
    }

    public async Task<Result<bool>> Handle(
        RestoreBranchCommand request,
        CancellationToken cancellationToken)
    {
        var tenantId = _currentTenant.GetTenantId();

        if (tenantId == Guid.Empty)
            return Error.NotFound("Tenant", "Tenant context not resolved.");

        // 1️⃣ FETCH DELETED BRANCH
        var branchResult = await _dbContext.GetEntityAsync<Branch>(
            b => b.Id == request.BranchId &&
                 b.TenantId == tenantId &&
                 b.IsDeleted,
            nameof(Branch),
            request.BranchId.ToString(),
            cancellationToken);

        if (branchResult.IsFailure)
            return branchResult.Error;

        var branch = branchResult.Value;

        // 2️⃣ UNIQUENESS CHECKS (RESTORE SE PEHLE)
        var checks = new (Expression<Func<Branch, bool>> Predicate, string Message)[]
        {
            (
                b => b.Code == branch.Code &&
                     b.TenantId == tenantId &&
                     b.Id != request.BranchId &&
                     !b.IsDeleted,
                $"Branch code '{branch.Code}' is already used by another active branch in this tenant. Cannot restore."
            )
        };

        var error = await _dbContext.EnsureAllUniqueAsync(
            checks,
            cancellationToken);

        if (error is not null)
            return error;

        // 3️⃣ RESTORE
        branch.IsDeleted = false;
        branch.DeletedAt = null;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(
            true,
            $"Branch '{branch.Name}' restored successfully.");
    }
}