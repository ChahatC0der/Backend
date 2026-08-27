using SchoolERP.Domain.Common;

namespace SchoolERP.Domain.Rbac.Entities;

public class Module : BaseEntity
{
    public long? ParentId { get; set; }
    public Guid? TenantId { get; set; }   // null for built-in, custom tenant modules
    public string Name { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsSystem { get; set; }
    public int SortOrder { get; set; }

    public Module? Parent { get; set; }
    public ICollection<Module> Children { get; set; } = new List<Module>();
    public ICollection<Permission> Permissions { get; set; } = new List<Permission>();
}