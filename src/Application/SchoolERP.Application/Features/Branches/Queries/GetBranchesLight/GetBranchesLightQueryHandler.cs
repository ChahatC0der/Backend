using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Branches.DTOs;
using SchoolERP.Domain.Shared.Results;
using SchoolERP.Domain.Tenants.Entities;

namespace SchoolERP.Application.Features.Branches.Queries.GetBranchesLight;

public class GetBranchesLightQueryHandler : IRequestHandler<GetBranchesLightQuery, Result<IEnumerable<BranchLightResponse>>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetBranchesLightQueryHandler(IApplicationDbContext dbContext)
        => _dbContext = dbContext;

    public async Task<Result<IEnumerable<BranchLightResponse>>> Handle(GetBranchesLightQuery request, CancellationToken cancellationToken)
    {
        var branches = await _dbContext.Set<Branch>()
            .Where(b => b.TenantId == request.TenantId && !b.IsDeleted)
            .OrderBy(b => b.Name)
            .Select(b => new BranchLightResponse(b.Id, b.Name, b.Code))
            .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<BranchLightResponse>>(branches);
    }
}