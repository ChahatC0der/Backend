using System.Security.Claims;
using SchoolERP.Application.Common.Interfaces;

namespace SchoolERP.Infrastructure.Services.AI;

public sealed class HttpAiBranchContextProvider
    : IAiBranchContextProvider
{
    public Guid? GetBranchId(ClaimsPrincipal principal)
    {
        // IMPORTANT:
        // The current backend source does not yet expose
        // a verified branch-context claim/header/provider.
        //
        // Therefore we intentionally return null instead
        // of guessing a claim name or request header.

        return null;
    }
}