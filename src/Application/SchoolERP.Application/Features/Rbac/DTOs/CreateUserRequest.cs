using System;
using System.Collections.Generic;
using System.Text;

namespace SchoolERP.Application.Features.Rbac.DTOs
{
    public record CreateUserRequest(
      Guid? TenantId,
      Guid? BranchId,
      string Name,
      string Email,
      string? Phone,
      string Password,
      bool IsPlatformAdmin = false,
      string Status = "active"
  );
}
