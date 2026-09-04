using SchoolERP.Domain.Common;

namespace SchoolERP.Domain.Rbac.Entities;

public class RolePermission : BaseTenantEntity
{
    public long RoleId { get; set; }
    public long PermissionId { get; set; }
    public long? CreatedBy { get; set; }

    // Navigation
    public Role Role { get; set; } = null!;
    public Permission Permission { get; set; } = null!;
}