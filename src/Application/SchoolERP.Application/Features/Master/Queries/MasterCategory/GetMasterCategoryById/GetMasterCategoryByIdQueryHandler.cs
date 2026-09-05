using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Master.DTOs;
using SchoolERP.Domain.Shared.Results;
using MasterCategoryEntity = SchoolERP.Domain.Master.Entities.MasterCategory;

namespace SchoolERP.Application.Features.Master.Queries.MasterCategory.GetMasterCategoryById;

public class GetMasterCategoryByIdQueryHandler : IRequestHandler<GetMasterCategoryByIdQuery, Result<MasterCategoryResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentTenantService _tenantService;

    public GetMasterCategoryByIdQueryHandler(IApplicationDbContext dbContext, ICurrentTenantService tenantService)
    {
        _dbContext = dbContext;
        _tenantService = tenantService;
    }

    public async Task<Result<MasterCategoryResponse>> Handle(GetMasterCategoryByIdQuery query, CancellationToken cancellationToken)
    {
        var tenantId = _tenantService.GetTenantId();

        var category = await _dbContext.Set<MasterCategoryEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(mc => mc.Id == query.Id && !mc.IsDeleted &&
                                       (mc.TenantId == null || mc.TenantId == tenantId), cancellationToken);

        if (category == null)
            return Error.NotFound("MasterCategory", query.Id.ToString());

        return Result.Success(category.Adapt<MasterCategoryResponse>());
    }
}