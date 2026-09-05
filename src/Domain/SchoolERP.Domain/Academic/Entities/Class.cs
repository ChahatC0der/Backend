using SchoolERP.Domain.Common;
using static System.Collections.Specialized.BitVector32;

namespace SchoolERP.Domain.Academic.Entities;

public class Class : BranchEntity,IMustHaveBranch
{
    public long? ClassGroupId { get; set; }                // optional FK
    public string Name { get; set; } = string.Empty;       // "Class 5"
    public byte Sequence { get; set; }                     // 1..12
    public bool IsActive { get; set; } = true;

    public ClassGroup? ClassGroup { get; set; }
    public ICollection<Section> Sections { get; set; } = new List<Section>();
}