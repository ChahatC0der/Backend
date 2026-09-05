using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Common.DTOs;
using SchoolERP.Application.Features.Master.DTOs;
using SchoolERP.Domain.Shared.Results;
using MasterCategoryEntity = SchoolERP.Domain.Master.Entities.MasterCategory;
using MasterItemEntity = SchoolERP.Domain.Master.Entities.MasterItem;

namespace SchoolERP.Application.Features.Master.Queries.MasterItem.GetMasterItems;

public class GetMasterItemsQueryHandler : IRequestHandler<GetMasterItemsQuery, Result<PagedResponse<MasterItemResponse>>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentTenantService _tenantService;

    public GetMasterItemsQueryHandler(IApplicationDbContext dbContext, ICurrentTenantService tenantService)
    {
        _dbContext = dbContext;
        _tenantService = tenantService;
    }

    public async Task<Result<PagedResponse<MasterItemResponse>>> Handle(GetMasterItemsQuery query, CancellationToken cancellationToken)
    {
        var tenantId = _tenantService.GetTenantId();
        var request = query.Request;

        // Join with Category to apply tenant filter
        var queryable = from item in _dbContext.Set<MasterItemEntity>()
                        join category in _dbContext.Set<MasterCategoryEntity>()
                            on item.CategoryId equals category.Id
                        where !item.IsDeleted && !category.IsDeleted &&
                              (category.TenantId == null || category.TenantId == tenantId)
                        select item;

        if (query.CategoryId.HasValue)
            queryable = queryable.Where(mi => mi.CategoryId == query.CategoryId.Value);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var search = request.SearchTerm.ToLower();
            queryable = queryable.Where(mi => mi.Value.ToLower().Contains(search) ||
                                             (mi.Code != null && mi.Code.ToLower().Contains(search)));
        }

        queryable = request.SortBy?.ToLower() switch
        {
            "value" => request.SortOrder?.ToLower() == "desc" ? queryable.OrderByDescending(mi => mi.Value) : queryable.OrderBy(mi => mi.Value),
            "code" => request.SortOrder?.ToLower() == "desc" ? queryable.OrderByDescending(mi => mi.Code) : queryable.OrderBy(mi => mi.Code),
            _ => queryable.OrderBy(mi => mi.SortOrder).ThenBy(mi => mi.Value)
        };

        var totalCount = await queryable.CountAsync(cancellationToken);
        var items = await queryable
            .Skip((request.Page - 1) * request.Size)
            .Take(request.Size)
            .ToListAsync(cancellationToken);

        var data = items.Select(x => x.Adapt<MasterItemResponse>()).ToList();

        return Result.Success(new PagedResponse<MasterItemResponse>
        {
            Data = data,
            TotalCount = totalCount,
            Page = request.Page,
            Size = request.Size
        });
    }
}