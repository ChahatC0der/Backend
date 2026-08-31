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
    private readonly ICurrentTenantService _tenantService;

    public GetBranchesLightQueryHandler(
        IApplicationDbContext dbContext,
        ICurrentTenantService tenantService)
    {
        _dbContext = dbContext;
        _tenantService = tenantService;
    }

    public async Task<Result<IEnumerable<BranchLightResponse>>> Handle(
        GetBranchesLightQuery request,
        CancellationToken cancellationToken)
    {
        var tenantId = _tenantService.GetTenantId();

        var branches = await _dbContext.Set<Branch>()
            .Where(b => b.TenantId == tenantId && !b.IsDeleted)
            .OrderBy(b => b.Name)
            .Select(b => new BranchLightResponse(b.Id, b.Name, b.Code))
            .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<BranchLightResponse>>(branches);
    }
}