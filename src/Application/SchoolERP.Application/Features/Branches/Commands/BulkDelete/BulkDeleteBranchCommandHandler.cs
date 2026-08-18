using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Domain.Shared.Results;
using SchoolERP.Domain.Tenants.Entities;

namespace SchoolERP.Application.Features.Branches.Commands.BulkDelete;

public class BulkDeleteBranchCommandHandler : IRequestHandler<BulkDeleteBranchCommand, Result<int>>
{
    private readonly IApplicationDbContext _dbContext;

    public BulkDeleteBranchCommandHandler(IApplicationDbContext dbContext) => _dbContext = dbContext;

    public async Task<Result<int>> Handle(BulkDeleteBranchCommand request, CancellationToken cancellationToken)
    {
        var branches = await _dbContext.Set<Branch>()
            .Where(b => b.TenantId == request.TenantId && request.Request.Ids.Contains(b.Id) && !b.IsDeleted)
            .ToListAsync(cancellationToken);

        if (!branches.Any())
            return Error.NotFound("Branches", "No matching branches found.");

        foreach (var branch in branches)
        {
            branch.IsDeleted = true;
            branch.DeletedAt = DateTime.UtcNow;
            branch.UpdatedAt = DateTime.UtcNow;
            branch.Status = "closed";
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(branches.Count, $"{branches.Count} branch(es) deleted successfully.");
    }
}