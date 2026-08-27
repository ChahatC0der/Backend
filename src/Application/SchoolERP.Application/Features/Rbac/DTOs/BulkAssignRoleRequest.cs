using System;
using System.Collections.Generic;
using System.Text;

namespace SchoolERP.Application.Features.Rbac.DTOs
{
    public record BulkAssignRoleRequest(
       Guid TenantId,
       long RoleId,
       string ScopeType,
       string? ScopeValue,
       List<long> UserIds
   );
}
