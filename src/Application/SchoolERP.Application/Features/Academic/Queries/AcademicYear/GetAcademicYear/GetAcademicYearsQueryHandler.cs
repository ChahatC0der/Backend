using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Common.DTOs;
using SchoolERP.Application.Features.Academic.DTOs;
using SchoolERP.Domain.Shared.Results;
using AcademicYearEntity = SchoolERP.Domain.Academic.Entities.AcademicYear;   // ✅ alias

namespace SchoolERP.Application.Features.Academic.Queries.AcademicYear.GetAcademicYears;

public class GetAcademicYearsQueryHandler : IRequestHandler<GetAcademicYearsQuery, Result<PagedResponse<AcademicYearResponse>>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentTenantService _tenantService;

    public GetAcademicYearsQueryHandler(IApplicationDbContext dbContext, ICurrentTenantService tenantService)
    {
        _dbContext = dbContext;
        _tenantService = tenantService;
    }

    public async Task<Result<PagedResponse<AcademicYearResponse>>> Handle(GetAcademicYearsQuery query, CancellationToken cancellationToken)
    {
        var branchId = _tenantService.GetBranchId();
        var request = query.Request;

        var queryable = _dbContext.Set<AcademicYearEntity>()
            .AsNoTracking()
            .Where(ay => ay.BranchId == branchId && !ay.IsDeleted);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var search = request.SearchTerm.ToLower();
            queryable = queryable.Where(ay => ay.Name.ToLower().Contains(search));
        }

        queryable = request.SortBy?.ToLower() switch
        {
            "name" => request.SortOrder?.ToLower() == "desc" ? queryable.OrderByDescending(ay => ay.Name) : queryable.OrderBy(ay => ay.Name),
            "startdate" => request.SortOrder?.ToLower() == "desc" ? queryable.OrderByDescending(ay => ay.StartDate) : queryable.OrderBy(ay => ay.StartDate),
            _ => queryable.OrderByDescending(ay => ay.CreatedAt)
        };

        var totalCount = await queryable.CountAsync(cancellationToken);
        var items = await queryable
            .Skip((request.Page - 1) * request.Size)
            .Take(request.Size)
            .ToListAsync(cancellationToken);

        var data = items.Select(x => x.Adapt<AcademicYearResponse>()).ToList();

        return Result.Success(new PagedResponse<AcademicYearResponse>
        {
            Data = data,
            TotalCount = totalCount,
            Page = request.Page,
            Size = request.Size
        });
    }
}