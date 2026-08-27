using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Common.DTOs;
using SchoolERP.Application.Features.Rbac.DTOs;
using SchoolERP.Domain.Rbac.Entities;
using SchoolERP.Domain.Shared.Results;
using RoleEntity = SchoolERP.Domain.Rbac.Entities.Role;

namespace SchoolERP.Application.Features.Rbac.Queries.Role.GetRoles;

public class GetRolesQueryHandler : IRequestHandler<GetRolesQuery, Result<PagedResponse<RoleResponse>>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetRolesQueryHandler(IApplicationDbContext dbContext) => _dbContext = dbContext;

    public async Task<Result<PagedResponse<RoleResponse>>> Handle(GetRolesQuery query, CancellationToken cancellationToken)
    {
        var request = query.Request;
        var rolesQuery = _dbContext.Set<RoleEntity>()
            .AsNoTracking()
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
                    .ThenInclude(p => p.Module)
            .Where(r => !r.IsDeleted);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var search = request.SearchTerm.ToLower();
            rolesQuery = rolesQuery.Where(r => r.Name.ToLower().Contains(search) ||
                                               r.Code.ToLower().Contains(search));
        }

        // Sorting
        rolesQuery = request.SortBy?.ToLower() switch
        {
            "name" => request.SortOrder?.ToLower() == "desc" ? rolesQuery.OrderByDescending(r => r.Name) : rolesQuery.OrderBy(r => r.Name),
            "code" => request.SortOrder?.ToLower() == "desc" ? rolesQuery.OrderByDescending(r => r.Code) : rolesQuery.OrderBy(r => r.Code),
            _ => rolesQuery.OrderBy(r => r.Id)
        };

        var totalCount = await rolesQuery.CountAsync(cancellationToken);
        var items = await rolesQuery
            .Skip((request.Page - 1) * request.Size)
            .Take(request.Size)
            .ToListAsync(cancellationToken);

        var data = items.Select(r => r.Adapt<RoleResponse>()).ToList();
        return Result.Success(new PagedResponse<RoleResponse>
        {
            Data = data,
            TotalCount = totalCount,
            Page = request.Page,
            Size = request.Size
        });
    }
}