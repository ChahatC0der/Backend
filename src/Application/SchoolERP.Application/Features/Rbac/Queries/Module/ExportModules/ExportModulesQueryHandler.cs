using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Extensions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Rbac.DTOs;
using SchoolERP.Domain.Shared.Results;
using ModuleEntity = SchoolERP.Domain.Rbac.Entities.Module;

namespace SchoolERP.Application.Features.Rbac.Queries.Module.ExportModules;

public class ExportModulesQueryHandler : IRequestHandler<ExportModulesQuery, Result<byte[]>>
{
    private readonly IApplicationDbContext _dbContext;

    public ExportModulesQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<byte[]>> Handle(ExportModulesQuery query, CancellationToken cancellationToken)
    {
        var items = await _dbContext.Set<ModuleEntity>()
            .AsNoTracking()
            .Where(m => !m.IsDeleted)
            .OrderBy(m => m.SortOrder).ThenBy(m => m.Name)
            .ProjectToType<ModuleLightResponse>()
            .ToListAsync(cancellationToken);

        var csvBytes = items.ToCsv();

        return Result.Success(csvBytes);
    }
}