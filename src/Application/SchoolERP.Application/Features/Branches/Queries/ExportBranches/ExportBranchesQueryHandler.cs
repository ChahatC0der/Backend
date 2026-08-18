using System.Text;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Domain.Shared.Results;
using SchoolERP.Domain.Tenants.Entities;

namespace SchoolERP.Application.Features.Branches.Queries.ExportBranches;

public class ExportBranchesQueryHandler : IRequestHandler<ExportBranchesQuery, Result<byte[]>>
{
    private readonly IApplicationDbContext _dbContext;

    public ExportBranchesQueryHandler(IApplicationDbContext dbContext) => _dbContext = dbContext;

    public async Task<Result<byte[]>> Handle(ExportBranchesQuery request, CancellationToken cancellationToken)
    {
        var branches = await _dbContext.Set<Branch>()
            .Where(b => b.TenantId == request.TenantId && !b.IsDeleted)
            .OrderBy(b => b.Name)
            .ToListAsync(cancellationToken);

        var csv = new StringBuilder();
        csv.AppendLine("Code,Name,Status,CreatedAt");

        foreach (var b in branches)
        {
            csv.AppendLine($"{b.Code},{b.Name},{b.Status},{b.CreatedAt:yyyy-MM-dd HH:mm:ss}");
        }

        return Result.Success(Encoding.UTF8.GetBytes(csv.ToString()));
    }
}