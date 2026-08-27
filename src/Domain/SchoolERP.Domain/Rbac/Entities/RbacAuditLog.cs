using SchoolERP.Domain.Common;

namespace SchoolERP.Domain.Rbac.Entities;

public class RbacAuditLog : TenantAuditableEntity
{
    public long PerformedBy { get; set; }
    public long? AffectedUserId { get; set; }
    public long? AffectedRoleId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string? Reason { get; set; }

    public User PerformedByUser { get; set; } = null!;
    public User? AffectedUser { get; set; }
    public Role? AffectedRole { get; set; }
}