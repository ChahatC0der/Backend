using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Rbac.DTOs;
using SchoolERP.Domain.Rbac.Entities;
using SchoolERP.Domain.Shared.Results;
using RoleEntity = SchoolERP.Domain.Rbac.Entities.Role;

namespace SchoolERP.Application.Features.Rbac.Queries.Role.GetAllRolesLight;

public class GetAllRolesLightQueryHandler : IRequestHandler<GetAllRolesLightQuery, Result<List<RoleLightResponse>>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetAllRolesLightQueryHandler(IApplicationDbContext dbContext) => _dbContext = dbContext;

    public async Task<Result<List<RoleLightResponse>>> Handle(GetAllRolesLightQuery query, CancellationToken cancellationToken)
    {
        var roles = await _dbContext.Set<RoleEntity>()
            .AsNoTracking()
            .Where(r => !r.IsDeleted)
            .OrderBy(r => r.Name)
            .ProjectToType<RoleLightResponse>()
            .ToListAsync(cancellationToken);

        return Result.Success(roles);
    }
}