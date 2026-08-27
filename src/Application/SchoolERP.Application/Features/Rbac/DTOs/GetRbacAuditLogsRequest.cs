using SchoolERP.Application.Common.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace SchoolERP.Application.Features.Rbac.DTOs
{
    public record GetRbacAuditLogsRequest : PagedRequest
    {
        public Guid? TenantId { get; init; }
        public long? UserId { get; init; }
        public long? RoleId { get; init; }
        public string? Action { get; init; }
        public DateTime? FromDate { get; init; }
        public DateTime? ToDate { get; init; }
    }
}
