using SchoolERP.Application.Common.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace SchoolERP.Application.Features.Rbac.DTOs
{
    public record BulkRoleJobResponse(
    long Id,
    Guid TenantId,
    string Status,
    int TotalUsers,
    int ProcessedCount,
    int FailedCount,
    DateTime CreatedAt,
    DateTime? CompletedAt
);
    
}
