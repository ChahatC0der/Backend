using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using SchoolERP.Application.Common.Interfaces;

namespace SchoolERP.Infrastructure.Services;

public class CurrentBranchService : ICurrentBranchService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentBranchService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? GetBranchId()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null) return null;

        // 1. Header override (X-Branch-Id)
        if (context.Request.Headers.TryGetValue("BranchId", out var headerValue) &&
            Guid.TryParse(headerValue, out var headerBranchId))
        {
            return headerBranchId;
        }

        // 2. JWT claim
        var branchClaim = context.User.FindFirst("BranchId")?.Value;
        if (Guid.TryParse(branchClaim, out var claimBranchId))
        {
            return claimBranchId;
        }

        return null;
    }

    public bool IsBranchResolved => GetBranchId().HasValue;
}