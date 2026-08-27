using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Domain.Shared.Results;
using SchoolERP.Domain.Tenants.Entities;

namespace SchoolERP.Application.Features.Tenants.Queries.ExportTenants;

public class ExportTenantsQueryHandler : IRequestHandler<ExportTenantsQuery, Result<byte[]>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IExcelExportService _excelExportService;   // 🔥 Sirf interface

    public ExportTenantsQueryHandler(IApplicationDbContext dbContext, IExcelExportService excelExportService)
    {
        _dbContext = dbContext;
        _excelExportService = excelExportService;
    }

    public async Task<Result<byte[]>> Handle(ExportTenantsQuery request, CancellationToken cancellationToken)
    {
        var tenants = await _dbContext.Set<Tenant>()
            .Where(t => !t.IsDeleted)
            .OrderBy(t => t.CreatedAt)
            .Select(t => new
            {
                t.Code,
                t.Name,
                t.Subdomain,
                Email = t.ContactEmail,
                t.Plan,
                t.Status,
                t.CreatedAt
            })
            .ToListAsync(cancellationToken);

        var bytes = _excelExportService.Export(tenants, "Tenants");   // 🔥 Interface ke through call
        return Result.Success(bytes);
    }
}