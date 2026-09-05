using SchoolERP.Domain.Common;

namespace SchoolERP.Domain.Master.Entities;

public class MasterItem : BaseEntity
{
    public long CategoryId { get; set; }
    public string Value { get; set; } = string.Empty;   // India, Hindu, Tuition
    public string? Code { get; set; }
    public string? Description { get; set; }
    public string? Metadata { get; set; }               // JSON string
    public int SortOrder { get; set; }
    public bool IsSystem { get; set; }
    public bool IsActive { get; set; } = true;

    public MasterCategory Category { get; set; } = null!;
}