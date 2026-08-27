using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Extensions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Branches.DTOs;
using SchoolERP.Domain.Shared.Results;
using SchoolERP.Domain.Tenants.Entities;
using System.Linq.Expressions;

namespace SchoolERP.Application.Features.Branches.Commands.UpdateBranch;

public class UpdateBranchCommandHandler : IRequestHandler<UpdateBranchCommand, Result<BranchResponse>>
{
    private readonly IApplicationDbContext _dbContext;

    public UpdateBranchCommandHandler(IApplicationDbContext dbContext)
        => _dbContext = dbContext;

    public async Task<Result<BranchResponse>> Handle(UpdateBranchCommand request, CancellationToken cancellationToken)
    {
        // 1️⃣ 🔥 FETCH USING HELPER
        var branchResult = await _dbContext.GetEntityByIdAsync<Branch>(request.BranchId, cancellationToken);
        if (branchResult.IsFailure)
            return branchResult.Error;

        var branch = branchResult.Value;

        // 2️⃣ CLEAN CODE
        var code = request.Request.Code.Trim().ToUpper();

        // 3️⃣ 🔥 UNIQUENESS CHECK (EXCLUDE CURRENT)
        var checks = new (Expression<Func<Branch, bool>> Predicate, string Message)[]
        {
            (b => b.Code == code && b.TenantId == request.TenantId && b.Id != request.BranchId && !b.IsDeleted,
                $"Branch code '{code}' already exists in this tenant.")
        };

        var error = await _dbContext.EnsureAllUniqueAsync(checks, cancellationToken);
        if (error is not null)
            return error;

        // 4️⃣ RESET OTHER DEFAULTS (if setting default)
        if (request.Request.IsDefault)
        {
            var existingDefaults = await _dbContext.Set<Branch>()
                .Where(b => b.TenantId == request.TenantId && b.IsDefault && b.Id != request.BranchId)
                .ToListAsync(cancellationToken);

            foreach (var item in existingDefaults)
                item.IsDefault = false;
        }

        // 5️⃣ 🔥 MAPSTER: UPDATE EXISTING
        request.Request.Adapt(branch);

        // 6️⃣ FORCE STATE CHANGE (so UpdatedAt gets set)
        //_dbContext.Entry(branch).State = EntityState.Modified;

        await _dbContext.SaveChangesAsync(cancellationToken);

        // 7️⃣ 🔥 RESPONSE
        var response = branch.Adapt<BranchResponse>();

        return Result.Success(response, "Branch updated successfully.");
    }
}