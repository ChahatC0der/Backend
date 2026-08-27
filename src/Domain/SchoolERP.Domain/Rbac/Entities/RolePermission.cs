using SchoolERP.Domain.Common;

namespace SchoolERP.Domain.Rbac.Entities;

public class RolePermission : TenantAuditableEntity
{
    public long RoleId { get; set; }
    public long PermissionId { get; set; }

    public Role Role { get; set; } = null!;
    public Permission Permission { get; set; } = null!;
}