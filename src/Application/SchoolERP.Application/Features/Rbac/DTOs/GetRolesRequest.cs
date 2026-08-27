using SchoolERP.Application.Common.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace SchoolERP.Application.Features.Rbac.DTOs
{
    public record GetRolesRequest : PagedRequest
    {
        public Guid? TenantId { get; init; }
        public bool IncludeDeleted { get; init; } = false;
    }
}
