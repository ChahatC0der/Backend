using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Academic.DTOs;
using SchoolERP.Domain.Shared.Results;
using ClassGroupEntity = SchoolERP.Domain.Academic.Entities.ClassGroup;

namespace SchoolERP.Application.Features.Academic.Queries.ClassGroup.GetClassGroupsLight;

public class GetClassGroupsLightQueryHandler : IRequestHandler<GetClassGroupsLightQuery, Result<List<ClassGroupLightResponse>>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentTenantService _tenantService;

    public GetClassGroupsLightQueryHandler(IApplicationDbContext dbContext, ICurrentTenantService tenantService)
    {
        _dbContext = dbContext;
        _tenantService = tenantService;
    }

    public async Task<Result<List<ClassGroupLightResponse>>> Handle(GetClassGroupsLightQuery query, CancellationToken cancellationToken)
    {
        var branchId = _tenantService.GetBranchId();

        var items = await _dbContext.Set<ClassGroupEntity>()
            .AsNoTracking()
            .Where(cg => cg.BranchId == branchId && !cg.IsDeleted)
            .OrderBy(cg => cg.Sequence)
            .ProjectToType<ClassGroupLightResponse>()
            .ToListAsync(cancellationToken);

        return Result.Success(items);
    }
}