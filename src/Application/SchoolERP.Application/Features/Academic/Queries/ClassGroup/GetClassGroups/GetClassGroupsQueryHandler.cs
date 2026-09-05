using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Common.DTOs;
using SchoolERP.Application.Features.Academic.DTOs;
using SchoolERP.Domain.Shared.Results;
using ClassGroupEntity = SchoolERP.Domain.Academic.Entities.ClassGroup;   // ✅ alias

namespace SchoolERP.Application.Features.Academic.Queries.ClassGroup.GetClassGroups;

public class GetClassGroupsQueryHandler : IRequestHandler<GetClassGroupsQuery, Result<PagedResponse<ClassGroupResponse>>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentTenantService _tenantService;

    public GetClassGroupsQueryHandler(IApplicationDbContext dbContext, ICurrentTenantService tenantService)
    {
        _dbContext = dbContext;
        _tenantService = tenantService;
    }

    public async Task<Result<PagedResponse<ClassGroupResponse>>> Handle(GetClassGroupsQuery query, CancellationToken cancellationToken)
    {
        var branchId = _tenantService.GetBranchId();
        var request = query.Request;

        var queryable = _dbContext.Set<ClassGroupEntity>()
            .AsNoTracking()
            .Where(cg => cg.BranchId == branchId && !cg.IsDeleted);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var search = request.SearchTerm.ToLower();
            queryable = queryable.Where(cg => cg.Name.ToLower().Contains(search) ||
                                             (cg.Description != null && cg.Description.ToLower().Contains(search)));
        }

        queryable = request.SortBy?.ToLower() switch
        {
            "name" => request.SortOrder?.ToLower() == "desc" ? queryable.OrderByDescending(cg => cg.Name) : queryable.OrderBy(cg => cg.Name),
            "sequence" => request.SortOrder?.ToLower() == "desc" ? queryable.OrderByDescending(cg => cg.Sequence) : queryable.OrderBy(cg => cg.Sequence),
            _ => queryable.OrderBy(cg => cg.Sequence)
        };

        var totalCount = await queryable.CountAsync(cancellationToken);
        var items = await queryable
            .Skip((request.Page - 1) * request.Size)
            .Take(request.Size)
            .ToListAsync(cancellationToken);

        var data = items.Select(x => x.Adapt<ClassGroupResponse>()).ToList();

        return Result.Success(new PagedResponse<ClassGroupResponse>
        {
            Data = data,
            TotalCount = totalCount,
            Page = request.Page,
            Size = request.Size
        });
    }
}