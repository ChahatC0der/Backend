using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Domain.Shared.Results;
using SchoolERP.Domain.Tenants.Entities;

namespace SchoolERP.Application.Features.Branches.Commands.RestoreBranch;

public class RestoreBranchCommandHandler : IRequestHandler<RestoreBranchCommand, Result<bool>>
{
    private readonly IApplicationDbContext _dbContext;

    public RestoreBranchCommandHandler(IApplicationDbContext dbContext) => _dbContext = dbContext;

    public async Task<Result<bool>> Handle(RestoreBranchCommand request, CancellationToken cancellationToken)
    {
        var branch = await _dbContext.Set<Branch>()
            .FirstOrDefaultAsync(b => b.Id == request.BranchId && b.TenantId == request.TenantId && b.IsDeleted, cancellationToken);

        if (branch == null)
            return Error.NotFound("Branch", request.BranchId.ToString());

        branch.IsDeleted = false;
        branch.DeletedAt = null;
        branch.UpdatedAt = DateTime.UtcNow;
        branch.Status = "active";

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(true, $"Branch '{branch.Name}' restored successfully.");
    }
}