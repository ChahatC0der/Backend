using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Rbac.DTOs;
using UserEntity = SchoolERP.Domain.Rbac.Entities.User;
using UserRoleEntity = SchoolERP.Domain.Rbac.Entities.UserRole;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Rbac.Queries.User.GetUserRoles;

public class GetUserRolesQueryHandler : IRequestHandler<GetUserRolesQuery, Result<List<RoleAssignmentResponse>>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetUserRolesQueryHandler(IApplicationDbContext dbContext) => _dbContext = dbContext;

    public async Task<Result<List<RoleAssignmentResponse>>> Handle(GetUserRolesQuery query, CancellationToken cancellationToken)
    {
        var userExists = await _dbContext.Set<UserEntity>()
            .AnyAsync(u => u.Id == query.UserId && !u.IsDeleted, cancellationToken);
        if (!userExists)
            return Error.NotFound("User", query.UserId.ToString());

        var assignments = await _dbContext.Set<UserRoleEntity>()
            .AsNoTracking()
            .Include(ur => ur.Role)
            .Where(ur => ur.UserId == query.UserId &&
                         ur.ValidFrom <= DateTime.UtcNow.Date &&
                         (ur.ValidTo == null || ur.ValidTo >= DateTime.UtcNow.Date))
            .ToListAsync(cancellationToken);

        var roles = assignments.Select(ur => ur.Adapt<RoleAssignmentResponse>()).ToList();
        return Result.Success(roles);
    }
}