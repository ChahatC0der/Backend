using System;
using System.Collections.Generic;
using System.Text;

namespace SchoolERP.Application.Features.Rbac.DTOs
{
    public record UserResponse(
    long Id,
    Guid? TenantId,
    Guid? BranchId,
    string Name,
    string Email,
    string? Phone,
    bool IsPlatformAdmin,
    string Status,
    int PermissionsVersion
);
}
