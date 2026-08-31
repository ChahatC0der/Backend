using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Extensions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Branches.DTOs;
using SchoolERP.Domain.Shared.Results;
using SchoolERP.Domain.Tenants.Entities;
using System.Linq.Expressions;

namespace SchoolERP.Application.Features.Branches.Commands.PatchBranch;

public class PatchBranchCommandHandler : IRequestHandler<PatchBranchCommand, Result<BranchResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentTenantService _currentTenant;

    public PatchBranchCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentTenantService currentTenant)
    {
        _dbContext = dbContext;
        _currentTenant = currentTenant;
    }

    public async Task<Result<BranchResponse>> Handle(
        PatchBranchCommand request,
        CancellationToken cancellationToken)
    {
        var tenantId = _currentTenant.GetTenantId();

        if (tenantId == Guid.Empty)
            return Error.NotFound("Tenant", "Tenant context not resolved.");

        // 1️⃣ FETCH
        var branchResult = await _dbContext.GetEntityByIdAsync<Branch>(
            request.BranchId,
            cancellationToken);

        if (branchResult.IsFailure)
            return branchResult.Error;

        var branch = branchResult.Value;

        // 2️⃣ UNIQUENESS CHECKS (ONLY IF CODE PROVIDED)
        var checks = new List<(Expression<Func<Branch, bool>> Predicate, string Message)>();

        if (!string.IsNullOrEmpty(request.Request.Code))
        {
            var code = request.Request.Code.Trim().ToUpper();

            checks.Add((
                b => b.Code == code &&
                     b.TenantId == tenantId &&
                     b.Id != request.BranchId &&
                     !b.IsDeleted,
                $"Branch code '{code}' already exists in this tenant."
            ));
        }

        if (checks.Any())
        {
            var error = await _dbContext.EnsureAllUniqueAsync(
                checks,
                cancellationToken);

            if (error is not null)
                return error;
        }

        // 3️⃣ RESET OTHER DEFAULTS (if setting default)
        if (request.Request.IsDefault.HasValue &&
            request.Request.IsDefault.Value)
        {
            var existingDefaults = await _dbContext.Set<Branch>()
                .Where(b =>
                    b.TenantId == tenantId &&
                    b.IsDefault &&
                    b.Id != request.BranchId)
                .ToListAsync(cancellationToken);

            foreach (var item in existingDefaults)
                item.IsDefault = false;
        }

        // 4️⃣ 🔥 MAPSTER: PATCH
        request.Request.Adapt(branch);

        // 5️⃣ SAVE
        await _dbContext.SaveChangesAsync(cancellationToken);

        // 6️⃣ RESPONSE
        var response = branch.Adapt<BranchResponse>();

        return Result.Success(
            response,
            "Branch updated successfully.");
    }
}