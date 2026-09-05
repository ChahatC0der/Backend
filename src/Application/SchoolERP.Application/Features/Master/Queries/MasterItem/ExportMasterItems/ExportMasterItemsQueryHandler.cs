using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Extensions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Master.DTOs;
using SchoolERP.Domain.Shared.Results;
using MasterCategoryEntity = SchoolERP.Domain.Master.Entities.MasterCategory;
using MasterItemEntity = SchoolERP.Domain.Master.Entities.MasterItem;

namespace SchoolERP.Application.Features.Master.Queries.MasterItem.ExportMasterItems;

public class ExportMasterItemsQueryHandler : IRequestHandler<ExportMasterItemsQuery, Result<byte[]>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentTenantService _tenantService;

    public ExportMasterItemsQueryHandler(IApplicationDbContext dbContext, ICurrentTenantService tenantService)
    {
        _dbContext = dbContext;
        _tenantService = tenantService;
    }

    public async Task<Result<byte[]>> Handle(ExportMasterItemsQuery query, CancellationToken cancellationToken)
    {
        var tenantId = _tenantService.GetTenantId();

        var category = await _dbContext.Set<MasterCategoryEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == query.CategoryId && !c.IsDeleted &&
                                      (c.TenantId == null || c.TenantId == tenantId), cancellationToken);
        if (category == null)
            return Error.NotFound("MasterCategory", query.CategoryId.ToString());

        var items = await _dbContext.Set<MasterItemEntity>()
            .AsNoTracking()
            .Where(mi => mi.CategoryId == query.CategoryId && !mi.IsDeleted)
            .OrderBy(mi => mi.SortOrder).ThenBy(mi => mi.Value)
            .ProjectToType<MasterItemLightResponse>()
            .ToListAsync(cancellationToken);

        var csvBytes = items.ToCsv();

        return Result.Success(csvBytes);
    }
}