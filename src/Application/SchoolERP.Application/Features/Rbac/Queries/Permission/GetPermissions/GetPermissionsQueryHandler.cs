using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Rbac.DTOs;
using PermissionEntity = SchoolERP.Domain.Rbac.Entities.Permission;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Rbac.Queries.Permission.GetPermissions;

public class GetPermissionsQueryHandler : IRequestHandler<GetPermissionsQuery, Result<List<PermissionResponse>>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetPermissionsQueryHandler(IApplicationDbContext dbContext) => _dbContext = dbContext;

    public async Task<Result<List<PermissionResponse>>> Handle(GetPermissionsQuery query, CancellationToken cancellationToken)
    {
        var permissions = await _dbContext.Set<PermissionEntity>()
            .AsNoTracking()
            .Include(p => p.Module)
            .Where(p => !p.IsDeleted)
            .OrderBy(p => p.Module.SortOrder)
                .ThenBy(p => p.Key)
            .ProjectToType<PermissionResponse>()
            .ToListAsync(cancellationToken);

        return Result.Success(permissions);
    }
}