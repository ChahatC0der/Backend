using System;
using System.Collections.Generic;
using System.Text;

namespace SchoolERP.Application.Features.Rbac.DTOs
{
    public record RbacAuditLogResponse(
    long Id,
    Guid TenantId,
    long PerformedBy,
    long? AffectedUserId,
    long? AffectedRoleId,
    string Action,
    string? OldValues,
    string? NewValues,
    string? Reason,
    DateTime CreatedAt
);
}
