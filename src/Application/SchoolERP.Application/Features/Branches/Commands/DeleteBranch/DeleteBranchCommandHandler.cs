using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Domain.Shared.Results;
using SchoolERP.Domain.Tenants.Entities;

namespace SchoolERP.Application.Features.Branches.Commands.DeleteBranch;

public class DeleteBranchCommandHandler : IRequestHandler<DeleteBranchCommand, Result<bool>>
{
    private readonly IApplicationDbContext _dbContext;

    public DeleteBranchCommandHandler(IApplicationDbContext dbContext) => _dbContext = dbContext;

    public async Task<Result<bool>> Handle(DeleteBranchCommand request, CancellationToken cancellationToken)
    {
        var branch = await _dbContext.Set<Branch>()
            .FirstOrDefaultAsync(b => b.Id == request.BranchId && b.TenantId == request.TenantId && !b.IsDeleted, cancellationToken);

        if (branch == null)
            return Error.NotFound("Branch", request.BranchId.ToString());

        branch.IsDeleted = true;
        branch.DeletedAt = DateTime.UtcNow;
        branch.UpdatedAt = DateTime.UtcNow;
        branch.Status = "closed";

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(true, $"Branch '{branch.Name}' deleted successfully.");
    }
}