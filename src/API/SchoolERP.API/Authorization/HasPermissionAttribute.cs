using Microsoft.AspNetCore.Authorization;

namespace SchoolERP.API.Authorization;

public class HasPermissionAttribute : AuthorizeAttribute
{
    public HasPermissionAttribute(string permission)
        : base(policy: $"Permission_{permission}") // 👈 YAHI FIX HAI
    {
    }
}