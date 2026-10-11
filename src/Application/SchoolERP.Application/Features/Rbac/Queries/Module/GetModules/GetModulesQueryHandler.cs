using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Common.DTOs;
using SchoolERP.Application.Features.Rbac.DTOs;
using SchoolERP.Domain.Shared.Results;
using ModuleEntity = SchoolERP.Domain.Rbac.Entities.Module;

namespace SchoolERP.Application.Features.Rbac.Queries.Module.GetModules;

public class GetModulesQueryHandler : IRequestHandler<GetModulesQuery, Result<PagedResponse<ModuleResponse>>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentTenantService _tenantService;

    public GetModulesQueryHandler(IApplicationDbContext dbContext, ICurrentTenantService tenantService)
    {
        _dbContext = dbContext;
        _tenantService = tenantService;
    }

    public async Task<Result<PagedResponse<ModuleResponse>>> Handle(GetModulesQuery query, CancellationToken cancellationToken)
    {
        var tenantId = _tenantService.GetTenantId();
        var request = query.Request;

        // Global filter handles "TenantId == null OR == current"
        var queryable = _dbContext.Set<ModuleEntity>()
            .AsNoTracking()
            .Where(m => !m.IsDeleted);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var search = request.SearchTerm.ToLower();
            queryable = queryable.Where(m => m.Name.ToLower().Contains(search) ||
                                             m.Key.ToLower().Contains(search));
        }

        queryable = request.SortBy?.ToLower() switch
        {
            "name" => request.SortOrder?.ToLower() == "desc" ? queryable.OrderByDescending(m => m.Name) : queryable.OrderBy(m => m.Name),
            "key" => request.SortOrder?.ToLower() == "desc" ? queryable.OrderByDescending(m => m.Key) : queryable.OrderBy(m => m.Key),
            _ => queryable.OrderBy(m => m.SortOrder).ThenBy(m => m.Name)
        };

        var totalCount = await queryable.CountAsync(cancellationToken);
        var items = await queryable
            .Skip((request.Page - 1) * request.Size)
            .Take(request.Size)
            .ToListAsync(cancellationToken);

        var data = items.Select(x => x.Adapt<ModuleResponse>()).ToList();

        return Result.Success(new PagedResponse<ModuleResponse>
        {
            Data = data,
            TotalCount = totalCount,
            Page = request.Page,
            Size = request.Size
        });
    }
}