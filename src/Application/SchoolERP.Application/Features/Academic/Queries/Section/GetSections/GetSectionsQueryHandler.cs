using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Common.DTOs;
using SchoolERP.Application.Features.Academic.DTOs;
using SchoolERP.Domain.Shared.Results;
using SectionEntity = SchoolERP.Domain.Academic.Entities.Section;   // ✅ alias

namespace SchoolERP.Application.Features.Academic.Queries.Section.GetSections;

public class GetSectionsQueryHandler : IRequestHandler<GetSectionsQuery, Result<PagedResponse<SectionResponse>>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentTenantService _tenantService;

    public GetSectionsQueryHandler(IApplicationDbContext dbContext, ICurrentTenantService tenantService)
    {
        _dbContext = dbContext;
        _tenantService = tenantService;
    }

    public async Task<Result<PagedResponse<SectionResponse>>> Handle(GetSectionsQuery query, CancellationToken cancellationToken)
    {
        var branchId = _tenantService.GetBranchId();
        var request = query.Request;

        var queryable = _dbContext.Set<SectionEntity>()
            .AsNoTracking()
            .Where(s => s.BranchId == branchId && !s.IsDeleted);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var search = request.SearchTerm.ToLower();
            queryable = queryable.Where(s => s.Name.ToLower().Contains(search));
        }

        queryable = request.SortBy?.ToLower() switch
        {
            "name" => request.SortOrder?.ToLower() == "desc" ? queryable.OrderByDescending(s => s.Name) : queryable.OrderBy(s => s.Name),
            _ => queryable.OrderBy(s => s.ClassId).ThenBy(s => s.Name)
        };

        var totalCount = await queryable.CountAsync(cancellationToken);
        var items = await queryable
            .Skip((request.Page - 1) * request.Size)
            .Take(request.Size)
            .ToListAsync(cancellationToken);

        var data = items.Select(x => x.Adapt<SectionResponse>()).ToList();

        return Result.Success(new PagedResponse<SectionResponse>
        {
            Data = data,
            TotalCount = totalCount,
            Page = request.Page,
            Size = request.Size
        });
    }
}