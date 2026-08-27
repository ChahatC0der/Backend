using System;
using System.Collections.Generic;
using System.Text;

namespace SchoolERP.Application.Features.Rbac.DTOs
{
    public record UpdateUserRequest(
    long Id,
    string Name,
    string? Phone,
    Guid? TenantId,
    Guid? BranchId,
    string? Status
);
}
