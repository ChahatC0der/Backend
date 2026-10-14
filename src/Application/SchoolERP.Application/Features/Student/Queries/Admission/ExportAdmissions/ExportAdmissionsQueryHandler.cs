using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Student.DTOs;
using SchoolERP.Domain.Shared.Results;
using AdmissionEntity = SchoolERP.Domain.Student.Admission.Entities.Admission;

namespace SchoolERP.Application.Features.Student.Queries.Admission.ExportAdmissions;

public class ExportAdmissionsQueryHandler : IRequestHandler<ExportAdmissionsQuery, Result<byte[]>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentBranchService _branchService;
    private readonly IExcelExportService _excelExportService;

    public ExportAdmissionsQueryHandler(
        IApplicationDbContext dbContext,
        ICurrentBranchService branchService,
        IExcelExportService excelExportService)
    {
        _dbContext = dbContext;
        _branchService = branchService;
        _excelExportService = excelExportService;
    }

    public async Task<Result<byte[]>> Handle(ExportAdmissionsQuery query, CancellationToken cancellationToken)
    {
        var branchId = _branchService.GetBranchId();
        if (branchId == null) return Error.Conflict("Please Choose a Branch");

        var items = await _dbContext.Set<AdmissionEntity>()
            .AsNoTracking()
            .Where(a => a.BranchId == branchId && !a.IsDeleted)
            .OrderByDescending(a => a.CreatedAt)
            .ProjectToType<AdmissionLightResponse>()
            .ToListAsync(cancellationToken);

        var excelExport = _excelExportService.Export(items, "Admissions");

        return Result.Success(excelExport);
    }
}
