using SchoolERP.Domain.Common;

namespace SchoolERP.Domain.Rbac.Entities;

public class Permission : BaseEntity
{
    public long ModuleId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public string? Description { get; set; }

    public Module Module { get; set; } = null!;
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}