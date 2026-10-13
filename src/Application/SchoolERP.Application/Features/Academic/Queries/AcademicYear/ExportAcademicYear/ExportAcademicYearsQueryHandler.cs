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
    private readonly ICurrentBranchService _branchService;
    private readonly IExcelExportService _excelExportService;

    public ExportAcademicYearsQueryHandler(IApplicationDbContext dbContext, ICurrentTenantService tenantService,ICurrentBranchService branchService, IExcelExportService excelExportService)
    {
        _dbContext = dbContext;
        _tenantService = tenantService;
        _branchService = branchService;
        _excelExportService = excelExportService;
    }

    public async Task<Result<byte[]>> Handle(ExportAcademicYearsQuery query, CancellationToken cancellationToken)
    {
        var branchId = _branchService.GetBranchId();
        if (branchId == null) return Error.Conflict("Please Choose a Branch");


        var items = await _dbContext.Set<AcademicYearEntity>()
            .AsNoTracking()
            .Where(ay => ay.BranchId == branchId && !ay.IsDeleted)
            .OrderBy(ay => ay.StartDate)
            .ProjectToType<AcademicYearLightResponse>()
            .ToListAsync(cancellationToken);

        var excelExport = _excelExportService.Export(items, "AcademicYears");

        //var csvBytes = items.ToCsv();

        return Result.Success(excelExport);
    }
}