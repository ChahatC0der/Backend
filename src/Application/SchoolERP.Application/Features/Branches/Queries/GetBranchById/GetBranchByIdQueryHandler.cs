using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Branches.DTOs;
using SchoolERP.Domain.Shared.Results;
using SchoolERP.Domain.Tenants.Entities;

namespace SchoolERP.Application.Features.Branches.Queries.GetBranchById;

public class GetBranchByIdQueryHandler : IRequestHandler<GetBranchByIdQuery, Result<BranchResponse>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetBranchByIdQueryHandler(IApplicationDbContext dbContext) => _dbContext = dbContext;

    public async Task<Result<BranchResponse>> Handle(GetBranchByIdQuery request, CancellationToken cancellationToken)
    {
        var branch = await _dbContext.Set<Branch>()
            .FirstOrDefaultAsync(b => b.Id == request.BranchId && b.TenantId == request.TenantId && !b.IsDeleted, cancellationToken);

        if (branch == null)
            return Error.NotFound("Branch", request.BranchId.ToString());

        return Result.Success(branch.Adapt<BranchResponse>());
    }
}