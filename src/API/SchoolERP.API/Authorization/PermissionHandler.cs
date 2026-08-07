using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace SchoolERP.API.Authorization;

public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly ILogger<PermissionHandler> _logger;

    public PermissionHandler(ILogger<PermissionHandler> logger)
    {
        _logger = logger;
    }

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        // 🔥 1. Check if user is authenticated
        if (!context.User.Identity?.IsAuthenticated ?? false)
        {
            _logger.LogWarning("Unauthenticated user attempted to access {Permission}", requirement.Permission);
            context.Fail();
            return Task.CompletedTask;
        }

        // 🔥 2. Check if user has the required permission claim
        var hasPermission = context.User.Claims.Any(c =>
            c.Type == "permission" && c.Value == requirement.Permission);

        if (hasPermission)
        {
            _logger.LogDebug("User {User} has permission {Permission}",
                context.User.FindFirst(ClaimTypes.Email)?.Value,
                requirement.Permission);
            context.Succeed(requirement);
        }
        else
        {
            _logger.LogWarning("User {User} missing permission {Permission}",
                context.User.FindFirst(ClaimTypes.Email)?.Value,
                requirement.Permission);
            context.Fail();
        }

        return Task.CompletedTask;
    }
}