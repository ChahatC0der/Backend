using SchoolERP.Domain.Common;

namespace SchoolERP.Domain.Student.Entities;

public class Parent : BaseEntity
{
    public Guid TenantId { get; set; }
    public long StudentId { get; set; }

    public string ParentType { get; set; } = string.Empty;      // father, mother, guardian, other
    public string FirstName { get; set; } = string.Empty;
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? Mobile { get; set; }
    public string? Occupation { get; set; }
    public bool IsPrimary { get; set; }
    public long? UserId { get; set; }                            // parent portal login (optional)

    // Navigation
    public Student Student { get; set; } = null!;
}
