using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Extensions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Academic.DTOs;
using SchoolERP.Domain.Shared.Results;
using AcademicYearEntity = SchoolERP.Domain.Academic.Entities.AcademicYear;

namespace SchoolERP.Application.Features.Academic.Queries.AcademicYear.ExportAcademicYears;

public class ExportAcademicYearsQueryHandler : IRequestHandler<ExportAcademicYearsQuery, Result<byte[]>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentTenantService _tenantService;

    public ExportAcademicYearsQueryHandler(IApplicationDbContext dbContext, ICurrentTenantService tenantService)
    {
        _dbContext = dbContext;
        _tenantService = tenantService;
    }

    public async Task<Result<byte[]>> Handle(ExportAcademicYearsQuery query, CancellationToken cancellationToken)
    {
        var branchId = _tenantService.GetBranchId();

        var items = await _dbContext.Set<AcademicYearEntity>()
            .AsNoTracking()
            .Where(ay => ay.BranchId == branchId && !ay.IsDeleted)
            .OrderBy(ay => ay.StartDate)
            .ProjectToType<AcademicYearLightResponse>()
            .ToListAsync(cancellationToken);

        var csvBytes = items.ToCsv();

        return Result.Success(csvBytes);
    }
}