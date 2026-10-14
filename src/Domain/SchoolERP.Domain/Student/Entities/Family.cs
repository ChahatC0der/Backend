using SchoolERP.Domain.Common;

namespace SchoolERP.Domain.Student.Entities;

public class Family : BranchEntity
{
    public Guid TenantId { get; set; }
    public string? HeadName { get; set; }
    public string? HeadMobile { get; set; }
    public string? HeadEmail { get; set; }

    // Navigation
    public ICollection<Student> Students { get; set; } = new List<Student>();
}
