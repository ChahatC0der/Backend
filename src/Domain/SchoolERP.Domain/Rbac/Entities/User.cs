using SchoolERP.Domain.Common;

namespace SchoolERP.Domain.Rbac.Entities;

public class User : BaseEntity
{
    public Guid? TenantId { get; set; }
    public Guid? BranchId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string PasswordHash { get; set; } = string.Empty;
    public bool IsPlatformAdmin { get; set; }
    public string Status { get; set; } = "active";   // active, inactive, suspended
    public int PermissionsVersion { get; set; } = 1;
    public DateTime? LastLogin { get; set; }

    // Navigation properties for RBAC (already added by you)
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<UserToken> UserTokens { get; set; } = new List<UserToken>();
    public ICollection<BulkRoleJob> BulkRoleJobs { get; set; } = new List<BulkRoleJob>();
    public ICollection<RbacAuditLog> RbacAuditLogsPerformed { get; set; } = new List<RbacAuditLog>();
    public ICollection<RbacAuditLog> RbacAuditLogsAffected { get; set; } = new List<RbacAuditLog>();
}