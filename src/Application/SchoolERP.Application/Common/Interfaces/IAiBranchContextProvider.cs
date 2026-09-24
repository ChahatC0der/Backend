using System.Security.Claims;

namespace SchoolERP.Application.Common.Interfaces;

public interface IAiBranchContextProvider
{
    Guid? GetBranchId(ClaimsPrincipal principal);
}