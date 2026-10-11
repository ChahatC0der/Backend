using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Rbac.DTOs;
using SchoolERP.Domain.Shared.Results;
using ModuleEntity = SchoolERP.Domain.Rbac.Entities.Module;

namespace SchoolERP.Application.Features.Rbac.Queries.Module.GetModulesLight;

public class GetModulesLightQueryHandler : IRequestHandler<GetModulesLightQuery, Result<List<ModuleLightResponse>>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetModulesLightQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<List<ModuleLightResponse>>> Handle(GetModulesLightQuery query, CancellationToken cancellationToken)
    {
        var items = await _dbContext.Set<ModuleEntity>()
            .AsNoTracking()
            .Where(m => !m.IsDeleted)
            .OrderBy(m => m.SortOrder).ThenBy(m => m.Name)
            .ProjectToType<ModuleLightResponse>()
            .ToListAsync(cancellationToken);

        return Result.Success(items);
    }
}