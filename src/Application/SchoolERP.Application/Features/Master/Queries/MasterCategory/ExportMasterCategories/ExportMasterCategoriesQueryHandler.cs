using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Extensions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Master.DTOs;
using SchoolERP.Domain.Shared.Results;
using MasterCategoryEntity = SchoolERP.Domain.Master.Entities.MasterCategory;

namespace SchoolERP.Application.Features.Master.Queries.MasterCategory.ExportMasterCategories;

public class ExportMasterCategoriesQueryHandler : IRequestHandler<ExportMasterCategoriesQuery, Result<byte[]>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentTenantService _tenantService;
    private readonly IExcelExportService _excelExportService;

    public ExportMasterCategoriesQueryHandler(IApplicationDbContext dbContext, ICurrentTenantService tenantService, IExcelExportService excelExportService)
    {
        _dbContext = dbContext;
        _tenantService = tenantService;
        _excelExportService = excelExportService;
    }

    public async Task<Result<byte[]>> Handle(ExportMasterCategoriesQuery query, CancellationToken cancellationToken)
    {
        var tenantId = _tenantService.GetTenantId();

        var items = await _dbContext.Set<MasterCategoryEntity>()
            .AsNoTracking()
            .Where(mc => !mc.IsDeleted && (mc.TenantId == null || mc.TenantId == tenantId))
            .OrderBy(mc => mc.Key)
            .ProjectToType<MasterCategoryLightResponse>()
            .ToListAsync(cancellationToken);

        var excelExport = _excelExportService.Export(items,"MasterCategories");
        //var csvBytes = items.ToCsv();

        return Result.Success(excelExport);
    }
}