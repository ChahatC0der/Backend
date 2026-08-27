using System;
using System.Collections.Generic;
using System.Text;

namespace SchoolERP.Application.Features.Rbac.DTOs
{
    public record CreatePermissionRequest(
      long ModuleId,
      string Action,
      string Key,
      string? Description
  );
}
