using SchoolERP.Domain.Common;

namespace SchoolERP.Domain.Rbac.Entities;

public class Role : BaseTenantEntity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsBuiltin { get; set; }
    public long? BaseRoleId { get; set; }
    public bool IsSystem { get; set; }

    public Role? BaseRole { get; set; }
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}