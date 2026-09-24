using System.Security.Claims;
using SchoolERP.Application.Common.Interfaces;

namespace SchoolERP.Infrastructure.Services.AI;

public sealed class HttpAiPermissionProvider
    : IAiPermissionProvider
{
    public IReadOnlySet<string> GetPermissions(
        ClaimsPrincipal principal)
    {
        // IMPORTANT:
        // The real permission claim/source has not been
        // verified from the current RBAC implementation.
        //
        // Therefore we intentionally return an empty set
        // instead of guessing a JWT claim name.

        return new HashSet<string>(
            StringComparer.OrdinalIgnoreCase);
    }
}