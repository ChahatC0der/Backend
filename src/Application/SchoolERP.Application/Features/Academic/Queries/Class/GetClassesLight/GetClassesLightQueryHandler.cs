using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Academic.DTOs;
using SchoolERP.Domain.Shared.Results;
using ClassEntity = SchoolERP.Domain.Academic.Entities.Class;

namespace SchoolERP.Application.Features.Academic.Queries.Class.GetClassesLight;

public class GetClassesLightQueryHandler : IRequestHandler<GetClassesLightQuery, Result<List<ClassLightResponse>>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentTenantService _tenantService;
    private readonly ICurrentBranchService _branchService;

    public GetClassesLightQueryHandler(IApplicationDbContext dbContext, ICurrentTenantService tenantService,ICurrentBranchService branchService)
    {
        _dbContext = dbContext;
        _tenantService = tenantService;
        _branchService = branchService;
    }

    public async Task<Result<List<ClassLightResponse>>> Handle(GetClassesLightQuery query, CancellationToken cancellationToken)
    {
        var branchId = _branchService.GetBranchId();

        var items = await _dbContext.Set<ClassEntity>()
            .AsNoTracking()
            .Where(c => c.BranchId == branchId && !c.IsDeleted)
            .OrderBy(c => c.Sequence)
            .ProjectToType<ClassLightResponse>()
            .ToListAsync(cancellationToken);

        return Result.Success(items);
    }
}