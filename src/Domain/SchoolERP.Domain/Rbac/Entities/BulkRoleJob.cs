using SchoolERP.Domain.Common;

namespace SchoolERP.Domain.Rbac.Entities;

public class BulkRoleJob : BaseTenantEntity
{
    public long CreatedBy { get; set; }
    public long RoleId { get; set; }
    public string ScopeType { get; set; } = string.Empty;
    public string ScopeValue { get; set; } = string.Empty;
    public int TotalUsers { get; set; }
    public int ProcessedCount { get; set; }
    public int FailedCount { get; set; }
    public string Status { get; set; } = "pending";
    public string? ErrorDetails { get; set; }
    public DateTime? CompletedAt { get; set; }

    public User RequestedBy { get; set; } = null!;
    public Role Role { get; set; } = null!;
}