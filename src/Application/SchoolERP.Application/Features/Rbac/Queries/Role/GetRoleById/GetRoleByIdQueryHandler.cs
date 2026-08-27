using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Rbac.DTOs;
using SchoolERP.Domain.Rbac.Entities;
using SchoolERP.Domain.Shared.Results;
using RoleEntity = SchoolERP.Domain.Rbac.Entities.Role;

namespace SchoolERP.Application.Features.Rbac.Queries.Role.GetRoleById;

public class GetRoleByIdQueryHandler : IRequestHandler<GetRoleByIdQuery, Result<RoleResponse>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetRoleByIdQueryHandler(IApplicationDbContext dbContext) => _dbContext = dbContext;

    public async Task<Result<RoleResponse>> Handle(GetRoleByIdQuery query, CancellationToken cancellationToken)
    {
        var role = await _dbContext.Set<RoleEntity>()
            .AsNoTracking()
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
                    .ThenInclude(p => p.Module)
            .FirstOrDefaultAsync(r => r.Id == query.RoleId && !r.IsDeleted, cancellationToken);

        if (role == null)
            return Error.NotFound("Role", query.RoleId.ToString());

        var response = role.Adapt<RoleResponse>();
        return Result.Success(response);
    }
}