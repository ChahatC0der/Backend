using SchoolERP.Domain.Common;

namespace SchoolERP.Domain.Rbac.Entities;

public class UserRole : TenantAuditableEntity
{
    public long UserId { get; set; }
    public long RoleId { get; set; }
    public string ScopeType { get; set; } = string.Empty;
    public string? ScopeValue { get; set; }
    public DateTime ValidFrom { get; set; } = DateTime.UtcNow.Date;
    public DateTime? ValidTo { get; set; }

    public User User { get; set; } = null!;
    public Role Role { get; set; } = null!;
}