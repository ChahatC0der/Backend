using System;
using System.Collections.Generic;
using System.Text;

namespace SchoolERP.Application.Features.Rbac.DTOs
{
    public record UpdatePermissionRequest(
    long Id,
    long ModuleId,
    string Action,
    string Key,
    string? Description
);
}
