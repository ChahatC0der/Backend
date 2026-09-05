using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Common.DTOs;
using SchoolERP.Application.Features.Master.DTOs;
using SchoolERP.Domain.Shared.Results;
using MasterCategoryEntity = SchoolERP.Domain.Master.Entities.MasterCategory;

namespace SchoolERP.Application.Features.Master.Queries.MasterCategory.GetMasterCategories;

public class GetMasterCategoriesQueryHandler : IRequestHandler<GetMasterCategoriesQuery, Result<PagedResponse<MasterCategoryResponse>>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentTenantService _tenantService;

    public GetMasterCategoriesQueryHandler(IApplicationDbContext dbContext, ICurrentTenantService tenantService)
    {
        _dbContext = dbContext;
        _tenantService = tenantService;
    }

    public async Task<Result<PagedResponse<MasterCategoryResponse>>> Handle(GetMasterCategoriesQuery query, CancellationToken cancellationToken)
    {
        var tenantId = _tenantService.GetTenantId();
        var request = query.Request;

        var queryable = _dbContext.Set<MasterCategoryEntity>()
            .AsNoTracking()
            .Where(mc => !mc.IsDeleted && (mc.TenantId == null || mc.TenantId == tenantId));

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var search = request.SearchTerm.ToLower();
            queryable = queryable.Where(mc => mc.Name.ToLower().Contains(search) ||
                                             mc.Key.ToLower().Contains(search));
        }

        queryable = request.SortBy?.ToLower() switch
        {
            "name" => request.SortOrder?.ToLower() == "desc" ? queryable.OrderByDescending(mc => mc.Name) : queryable.OrderBy(mc => mc.Name),
            "key" => request.SortOrder?.ToLower() == "desc" ? queryable.OrderByDescending(mc => mc.Key) : queryable.OrderBy(mc => mc.Key),
            _ => queryable.OrderBy(mc => mc.Id)
        };

        var totalCount = await queryable.CountAsync(cancellationToken);
        var items = await queryable
            .Skip((request.Page - 1) * request.Size)
            .Take(request.Size)
            .ToListAsync(cancellationToken);

        var data = items.Select(x => x.Adapt<MasterCategoryResponse>()).ToList();

        return Result.Success(new PagedResponse<MasterCategoryResponse>
        {
            Data = data,
            TotalCount = totalCount,
            Page = request.Page,
            Size = request.Size
        });
    }
}