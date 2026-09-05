using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Academic.DTOs;
using SchoolERP.Domain.Shared.Results;
using AcademicYearEntity = SchoolERP.Domain.Academic.Entities.AcademicYear;

namespace SchoolERP.Application.Features.Academic.Queries.AcademicYear.GetAcademicYearsLight;

public class GetAcademicYearsLightQueryHandler : IRequestHandler<GetAcademicYearsLightQuery, Result<List<AcademicYearLightResponse>>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentTenantService _tenantService;

    public GetAcademicYearsLightQueryHandler(IApplicationDbContext dbContext, ICurrentTenantService tenantService)
    {
        _dbContext = dbContext;
        _tenantService = tenantService;
    }

    public async Task<Result<List<AcademicYearLightResponse>>> Handle(GetAcademicYearsLightQuery query, CancellationToken cancellationToken)
    {
        var branchId = _tenantService.GetBranchId();

        var items = await _dbContext.Set<AcademicYearEntity>()
            .AsNoTracking()
            .Where(ay => ay.BranchId == branchId && !ay.IsDeleted)
            .OrderByDescending(ay => ay.StartDate)
            .ProjectToType<AcademicYearLightResponse>()
            .ToListAsync(cancellationToken);

        return Result.Success(items);
    }
}