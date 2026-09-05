using SchoolERP.Domain.Common;
using SchoolERP.Domain.Rbac.Entities;

namespace SchoolERP.Domain.Master.Entities;

public class MasterCategory : BaseEntity
{
    public long ModuleId { get; set; }
    public Guid? TenantId { get; set; }                 // NULL = Global, NOT NULL = Tenant-specific
    public string Key { get; set; } = string.Empty;     // religion, country, fee_head
    public string Name { get; set; } = string.Empty;    // Religions, Countries
    public string? Description { get; set; }
    public bool IsSystem { get; set; }
    public bool IsActive { get; set; } = true;

    public Module Module { get; set; } = null!;         // FK to RBAC Modules
    public ICollection<MasterItem> Items { get; set; } = new List<MasterItem>();
}