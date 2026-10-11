using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Rbac.DTOs;
using SchoolERP.Domain.Shared.Results;
using ModuleEntity = SchoolERP.Domain.Rbac.Entities.Module;

namespace SchoolERP.Application.Features.Rbac.Queries.Module.GetModuleById;

public class GetModuleByIdQueryHandler : IRequestHandler<GetModuleByIdQuery, Result<ModuleResponse>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetModuleByIdQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<ModuleResponse>> Handle(GetModuleByIdQuery query, CancellationToken cancellationToken)
    {
        var module = await _dbContext.Set<ModuleEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == query.Id && !m.IsDeleted, cancellationToken);

        if (module == null)
            return Error.NotFound("Module", query.Id.ToString());

        return Result.Success(module.Adapt<ModuleResponse>());
    }
}