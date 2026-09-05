using SchoolERP.Domain.Common;

namespace SchoolERP.Domain.Academic.Entities;

public class ClassGroup : BranchEntity,IMustHaveBranch
{
    public string Name { get; set; } = string.Empty;       // Primary, Middle, Secondary
    public byte Sequence { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}