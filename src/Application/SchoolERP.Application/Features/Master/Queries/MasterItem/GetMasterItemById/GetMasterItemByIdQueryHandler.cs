using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Master.DTOs;
using SchoolERP.Domain.Shared.Results;
using MasterCategoryEntity = SchoolERP.Domain.Master.Entities.MasterCategory;
using MasterItemEntity = SchoolERP.Domain.Master.Entities.MasterItem;

namespace SchoolERP.Application.Features.Master.Queries.MasterItem.GetMasterItemById;

public class GetMasterItemByIdQueryHandler : IRequestHandler<GetMasterItemByIdQuery, Result<MasterItemResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentTenantService _tenantService;

    public GetMasterItemByIdQueryHandler(IApplicationDbContext dbContext, ICurrentTenantService tenantService)
    {
        _dbContext = dbContext;
        _tenantService = tenantService;
    }

    public async Task<Result<MasterItemResponse>> Handle(GetMasterItemByIdQuery query, CancellationToken cancellationToken)
    {
        var tenantId = _tenantService.GetTenantId();

        var item = await (
            from mi in _dbContext.Set<MasterItemEntity>()
            join c in _dbContext.Set<MasterCategoryEntity>() on mi.CategoryId equals c.Id
            where mi.Id == query.Id && !mi.IsDeleted && !c.IsDeleted &&
                  (c.TenantId == null || c.TenantId == tenantId)
            select mi
        ).AsNoTracking().FirstOrDefaultAsync(cancellationToken);

        if (item == null)
            return Error.NotFound("MasterItem", query.Id.ToString());

        return Result.Success(item.Adapt<MasterItemResponse>());
    }
}