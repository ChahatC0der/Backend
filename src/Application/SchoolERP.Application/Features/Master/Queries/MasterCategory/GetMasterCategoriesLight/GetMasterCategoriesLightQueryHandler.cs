using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Master.DTOs;
using SchoolERP.Domain.Shared.Results;
using MasterCategoryEntity = SchoolERP.Domain.Master.Entities.MasterCategory;

namespace SchoolERP.Application.Features.Master.Queries.MasterCategory.GetMasterCategoriesLight;

public class GetMasterCategoriesLightQueryHandler : IRequestHandler<GetMasterCategoriesLightQuery, Result<List<MasterCategoryLightResponse>>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentTenantService _tenantService;

    public GetMasterCategoriesLightQueryHandler(IApplicationDbContext dbContext, ICurrentTenantService tenantService)
    {
        _dbContext = dbContext;
        _tenantService = tenantService;
    }

    public async Task<Result<List<MasterCategoryLightResponse>>> Handle(GetMasterCategoriesLightQuery query, CancellationToken cancellationToken)
    {
        var tenantId = _tenantService.GetTenantId();

        var items = await _dbContext.Set<MasterCategoryEntity>()
            .AsNoTracking()
            .Where(mc => !mc.IsDeleted && mc.IsActive && (mc.TenantId == null || mc.TenantId == tenantId))
            .OrderBy(mc => mc.Key)
            .ProjectToType<MasterCategoryLightResponse>()
            .ToListAsync(cancellationToken);

        return Result.Success(items);
    }
}