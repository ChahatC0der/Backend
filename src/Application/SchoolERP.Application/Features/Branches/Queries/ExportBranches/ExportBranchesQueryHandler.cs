using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Domain.Shared.Results;
using SchoolERP.Domain.Tenants.Entities;

namespace SchoolERP.Application.Features.Branches.Queries.ExportBranches;

public class ExportBranchesQueryHandler : IRequestHandler<ExportBranchesQuery, Result<byte[]>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IExcelExportService _excelExportService;
    private readonly ICurrentTenantService _tenantService;

    public ExportBranchesQueryHandler(
        IApplicationDbContext dbContext,
        IExcelExportService excelExportService,
        ICurrentTenantService tenantService)
    {
        _dbContext = dbContext;
        _excelExportService = excelExportService;
        _tenantService = tenantService;
    }

    public async Task<Result<byte[]>> Handle(
        ExportBranchesQuery request,
        CancellationToken cancellationToken)
    {
        var tenantId = _tenantService.GetTenantId();

        // 1️⃣ FETCH DATA
        var branches = await _dbContext.Set<Branch>()
            .Where(b => b.TenantId == tenantId && !b.IsDeleted)
            .OrderBy(b => b.Name)
            .Select(b => new
            {
                b.Code,
                b.Name,
                b.Status,
                b.CreatedAt,
                b.UpdatedAt
            })
            .ToListAsync(cancellationToken);

        // 2️⃣ GENERATE EXCEL
        var bytes = _excelExportService.Export(branches, "Branches");

        return Result.Success(bytes);
    }
}