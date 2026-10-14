using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Student.DTOs;
using SchoolERP.Domain.Shared.Results;
using FamilyEntity = SchoolERP.Domain.Student.Entities.Family;

namespace SchoolERP.Application.Features.Student.Queries.Family.ExportFamilies;

public class ExportFamiliesQueryHandler : IRequestHandler<ExportFamiliesQuery, Result<byte[]>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentBranchService _branchService;
    private readonly IExcelExportService _excelExportService;

    public ExportFamiliesQueryHandler(
        IApplicationDbContext dbContext,
        ICurrentBranchService branchService,
        IExcelExportService excelExportService)
    {
        _dbContext = dbContext;
        _branchService = branchService;
        _excelExportService = excelExportService;
    }

    public async Task<Result<byte[]>> Handle(ExportFamiliesQuery query, CancellationToken cancellationToken)
    {
        var branchId = _branchService.GetBranchId();
        if (branchId == null) return Error.Conflict("Please Choose a Branch");

        var items = await _dbContext.Set<FamilyEntity>()
            .AsNoTracking()
            .Include(f => f.Students)
            .Where(f => f.BranchId == branchId && !f.IsDeleted)
            .OrderBy(f => f.HeadName)
            .ProjectToType<FamilyLightResponse>()
            .ToListAsync(cancellationToken);

        var excelExport = _excelExportService.Export(items, "Families");

        return Result.Success(excelExport);
    }
}
