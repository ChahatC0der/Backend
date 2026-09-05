using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Common.DTOs;
using SchoolERP.Application.Features.Academic.DTOs;
using SchoolERP.Domain.Shared.Results;
using ClassEntity = SchoolERP.Domain.Academic.Entities.Class;   // ✅ alias

namespace SchoolERP.Application.Features.Academic.Queries.Class.GetClasses;

public class GetClassesQueryHandler : IRequestHandler<GetClassesQuery, Result<PagedResponse<ClassResponse>>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentTenantService _tenantService;

    public GetClassesQueryHandler(IApplicationDbContext dbContext, ICurrentTenantService tenantService)
    {
        _dbContext = dbContext;
        _tenantService = tenantService;
    }

    public async Task<Result<PagedResponse<ClassResponse>>> Handle(GetClassesQuery query, CancellationToken cancellationToken)
    {
        var branchId = _tenantService.GetBranchId();
        var request = query.Request;

        var queryable = _dbContext.Set<ClassEntity>()
            .AsNoTracking()
            .Where(c => c.BranchId == branchId && !c.IsDeleted);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var search = request.SearchTerm.ToLower();
            queryable = queryable.Where(c => c.Name.ToLower().Contains(search));
        }

        queryable = request.SortBy?.ToLower() switch
        {
            "name" => request.SortOrder?.ToLower() == "desc" ? queryable.OrderByDescending(c => c.Name) : queryable.OrderBy(c => c.Name),
            "sequence" => request.SortOrder?.ToLower() == "desc" ? queryable.OrderByDescending(c => c.Sequence) : queryable.OrderBy(c => c.Sequence),
            _ => queryable.OrderBy(c => c.Sequence)
        };

        var totalCount = await queryable.CountAsync(cancellationToken);
        var items = await queryable
            .Skip((request.Page - 1) * request.Size)
            .Take(request.Size)
            .ToListAsync(cancellationToken);

        var data = items.Select(x => x.Adapt<ClassResponse>()).ToList();

        return Result.Success(new PagedResponse<ClassResponse>
        {
            Data = data,
            TotalCount = totalCount,
            Page = request.Page,
            Size = request.Size
        });
    }
}