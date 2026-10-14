using SchoolERP.Domain.Common;
using StaffEntity = SchoolERP.Domain.Staff.Entities.Staff;

namespace SchoolERP.Domain.Student.Entities;

public class Parent : BaseEntity
{
    public Guid TenantId { get; set; }
    public long StudentId { get; set; }
    public long? StaffId { get; set; }                       // 🔥 NEW: link to Staff if parent is employee

    public string ParentType { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? Mobile { get; set; }
    public string? Occupation { get; set; }
    public bool IsPrimary { get; set; }
    public long? UserId { get; set; }

    // Navigation
    public Student Student { get; set; } = null!;
    public StaffEntity? Staff { get; set; }                  // 🔥 NEW
}
