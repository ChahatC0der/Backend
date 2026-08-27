using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Extensions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Domain.Shared.Results;
using SchoolERP.Domain.Tenants.Entities;

namespace SchoolERP.Application.Features.Branches.Commands.DeleteBranch;

public class DeleteBranchCommandHandler : IRequestHandler<DeleteBranchCommand, Result<bool>>
{
    private readonly IApplicationDbContext _dbContext;

    public DeleteBranchCommandHandler(IApplicationDbContext dbContext)
        => _dbContext = dbContext;

    public async Task<Result<bool>> Handle(DeleteBranchCommand request, CancellationToken cancellationToken)
    {
        // 1️⃣ FETCH
        var branchResult = await _dbContext.GetEntityByIdAsync<Branch>(request.BranchId, cancellationToken);
        if (branchResult.IsFailure)
            return branchResult.Error;

        var branch = branchResult.Value;

        // 2️⃣ SOFT DELETE
        branch.IsDeleted = true;
        branch.DeletedAt = DateTime.UtcNow;

        // 🔥 UpdatedAt auto-set by SaveChangesAsync
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(true, $"Branch '{branch.Name}' deleted successfully.");
    }
}