namespace SchoolERP.Domain.Rbac.Entities;

public class UserRole
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public long RoleId { get; set; }
    public string ScopeType { get; set; } = string.Empty;
    public string? ScopeValue { get; set; }
    public DateTime ValidFrom { get; set; } = DateTime.UtcNow.Date;
    public DateTime? ValidTo { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public long? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public long? UpdatedBy { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
    public Role Role { get; set; } = null!;
}