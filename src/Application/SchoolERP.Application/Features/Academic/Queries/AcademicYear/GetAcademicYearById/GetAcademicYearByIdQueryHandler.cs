using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Academic.DTOs;
using SchoolERP.Domain.Shared.Results;
using AcademicYearEntity = SchoolERP.Domain.Academic.Entities.AcademicYear;

namespace SchoolERP.Application.Features.Academic.Queries.AcademicYear.GetAcademicYearById;

public class GetAcademicYearByIdQueryHandler : IRequestHandler<GetAcademicYearByIdQuery, Result<AcademicYearResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentTenantService _tenantService;

    public GetAcademicYearByIdQueryHandler(IApplicationDbContext dbContext, ICurrentTenantService tenantService)
    {
        _dbContext = dbContext;
        _tenantService = tenantService;
    }

    public async Task<Result<AcademicYearResponse>> Handle(GetAcademicYearByIdQuery query, CancellationToken cancellationToken)
    {
        var branchId = _tenantService.GetBranchId();

        var academicYear = await _dbContext.Set<AcademicYearEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(ay => ay.Id == query.Id && ay.BranchId == branchId && !ay.IsDeleted, cancellationToken);

        if (academicYear == null)
            return Error.NotFound("AcademicYear", query.Id.ToString());

        return Result.Success(academicYear.Adapt<AcademicYearResponse>());
    }
}