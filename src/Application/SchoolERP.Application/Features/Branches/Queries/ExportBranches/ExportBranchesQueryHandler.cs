using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Domain.Shared.Results;
using SchoolERP.Domain.Tenants.Entities;

namespace SchoolERP.Application.Features.Branches.Queries.ExportBranches;

public class ExportBranchesQueryHandler : IRequestHandler<ExportBranchesQuery, Result<byte[]>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IExcelExportService _excelExportService; // 👈 INJECTED

    public ExportBranchesQueryHandler(
        IApplicationDbContext dbContext,
        IExcelExportService excelExportService)
    {
        _dbContext = dbContext;
        _excelExportService = excelExportService;
    }

    public async Task<Result<byte[]>> Handle(ExportBranchesQuery request, CancellationToken cancellationToken)
    {
        // 1️⃣ FETCH DATA (WITH UPDATEDAT)
        var branches = await _dbContext.Set<Branch>()
            .Where(b => b.TenantId == request.TenantId && !b.IsDeleted)
            .OrderBy(b => b.Name)
            .Select(b => new
            {
                b.Code,
                b.Name,
                b.Status,
                b.CreatedAt,
                b.UpdatedAt   // 👈 NAYA FIELD
            })
            .ToListAsync(cancellationToken);

        // 2️⃣ 🔥 GENERATE EXCEL (SERVICE CALL)
        var bytes = _excelExportService.Export(branches, "Branches");

        return Result.Success(bytes);
    }
}