using System.Security.Claims;

namespace SchoolERP.Application.Common.Interfaces;

public interface IAiPermissionProvider
{
    IReadOnlySet<string> GetPermissions(
        ClaimsPrincipal principal);
}