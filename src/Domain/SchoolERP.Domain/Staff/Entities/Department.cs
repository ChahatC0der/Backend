using SchoolERP.Domain.Common;

namespace SchoolERP.Domain.Staff.Entities;

public class Department : BranchEntity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public long? HodId { get; set; }                    // FK to Staff (nullable)

    // Navigation
    public Staff? Hod { get; set; }
    public ICollection<Staff> StaffMembers { get; set; } = new List<Staff>();
}