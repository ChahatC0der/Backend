using System.Text;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Domain.Shared.Results;
using SchoolERP.Domain.Tenants.Entities;

namespace SchoolERP.Application.Features.Tenants.Queries.ExportTenants;

public class ExportTenantsQueryHandler : IRequestHandler<ExportTenantsQuery, Result<byte[]>>
{
    private readonly IApplicationDbContext _dbContext;

    public ExportTenantsQueryHandler(IApplicationDbContext dbContext)
        => _dbContext = dbContext;

    public async Task<Result<byte[]>> Handle(ExportTenantsQuery request, CancellationToken cancellationToken)
    {
        var tenants = await _dbContext.Set<Tenant>()
            .Where(t => !t.IsDeleted)
            .OrderBy(t => t.CreatedAt)
            .ToListAsync(cancellationToken);

        var csv = new StringBuilder();
        csv.AppendLine("Code,Name,Subdomain,Email,Plan,Status,CreatedAt");

        foreach (var t in tenants)
        {
            csv.AppendLine($"{t.Code},{t.Name},{t.Subdomain},{t.ContactEmail},{t.Plan},{t.Status},{t.CreatedAt:yyyy-MM-dd HH:mm:ss}");
        }

        var bytes = Encoding.UTF8.GetBytes(csv.ToString());
        return Result.Success(bytes);
    }
}