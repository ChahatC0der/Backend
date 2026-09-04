using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Auth.DTOs;
using SchoolERP.Application.Features.Rbac.DTOs;
using SchoolERP.Domain.Rbac.Entities;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<LoginResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IJwtTokenService _jwtTokenService;

    public LoginCommandHandler(IApplicationDbContext dbContext, IJwtTokenService jwtTokenService)
    {
        _dbContext = dbContext;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<Result<LoginResponse>> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;

        var user = await _dbContext.Set<User>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email == request.Email && !u.IsDeleted, cancellationToken);

        if (user == null)
            return Error.Unauthorized("Invalid credentials.");

        // TODO: Replace with proper password hashing later
        if (user.PasswordHash != request.Password)
            return Error.Unauthorized("Invalid credentials.");

        // Get roles
        var roles = await _dbContext.Set<UserRole>()
            .Where(ur => ur.UserId == user.Id)
            .Select(ur => ur.Role.Name)
            .ToListAsync(cancellationToken);

        // Get permissions (direct + inherited)
        var permissionSet = new HashSet<string>();
        var assignments = await _dbContext.Set<UserRole>()
            .Include(ur => ur.Role)
                .ThenInclude(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
            .Where(ur => ur.UserId == user.Id &&
                         ur.ValidFrom <= DateTime.UtcNow.Date &&
                         (ur.ValidTo == null || ur.ValidTo >= DateTime.UtcNow.Date))
            .ToListAsync(cancellationToken);

        foreach (var assignment in assignments)
        {
            var role = assignment.Role;
            // Direct permissions
            foreach (var rp in role.RolePermissions)
                permissionSet.Add(rp.Permission.Key);

            // Inherited permissions (base role)
            var baseRoleId = role.BaseRoleId;
            while (baseRoleId.HasValue)
            {
                var baseRole = await _dbContext.Set<Role>()
                    .AsNoTracking()
                    .Include(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
                    .FirstOrDefaultAsync(r => r.Id == baseRoleId.Value, cancellationToken);
                if (baseRole == null) break;
                foreach (var rp in baseRole.RolePermissions)
                    permissionSet.Add(rp.Permission.Key);
                baseRoleId = baseRole.BaseRoleId;
            }
        }

        // Generate token with permissions
        var token = _jwtTokenService.GenerateToken(user, roles, permissionSet.ToList());

        var response = new LoginResponse(token, user.Adapt<UserResponse>());
        return Result.Success(response);
    }
}