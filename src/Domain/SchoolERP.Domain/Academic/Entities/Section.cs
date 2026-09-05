using SchoolERP.Domain.Common;

namespace SchoolERP.Domain.Academic.Entities;

public class Section : BranchEntity,IMustHaveBranch
{
    public long ClassId { get; set; }
    public string Name { get; set; } = string.Empty;       // "A", "B", "Morning"
    public int? Capacity { get; set; }

    public Class Class { get; set; } = null!;
}