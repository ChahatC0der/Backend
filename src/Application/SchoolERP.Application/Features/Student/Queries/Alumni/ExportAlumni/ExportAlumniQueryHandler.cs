using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Student.DTOs;
using SchoolERP.Domain.Shared.Results;
using AlumniEntity = SchoolERP.Domain.Student.Alumni.Entities.Alumni;

namespace SchoolERP.Application.Features.Student.Queries.Alumni.ExportAlumni;

public class ExportAlumniQueryHandler : IRequestHandler<ExportAlumniQuery, Result<byte[]>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentBranchService _branchService;
    private readonly IExcelExportService _excelExportService;

    public ExportAlumniQueryHandler(
        IApplicationDbContext dbContext,
        ICurrentBranchService branchService,
        IExcelExportService excelExportService)
    {
        _dbContext = dbContext;
        _branchService = branchService;
        _excelExportService = excelExportService;
    }

    public async Task<Result<byte[]>> Handle(ExportAlumniQuery query, CancellationToken cancellationToken)
    {
        var branchId = _branchService.GetBranchId();
        if (branchId == null) return Error.Conflict("Please Choose a Branch");

        var items = await _dbContext.Set<AlumniEntity>()
            .AsNoTracking()
            .Where(a => a.BranchId == branchId && !a.IsDeleted)
            .OrderBy(a => a.FirstName)
            .ProjectToType<AlumniLightResponse>()
            .ToListAsync(cancellationToken);

        var excelExport = _excelExportService.Export(items, "Alumni");

        return Result.Success(excelExport);
    }
}
