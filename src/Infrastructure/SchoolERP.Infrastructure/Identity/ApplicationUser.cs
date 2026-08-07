using Microsoft.AspNetCore.Identity;

namespace SchoolERP.Infrastructure.Identity;

public class ApplicationUser : IdentityUser<long>
{
    // 🔥 Custom fields from our existing "Users" DDL table
    public Guid TenantId { get; set; }
    public long? BranchId { get; set; }
    public string? Name { get; set; }
    public string? Phone { get; set; }
    public bool IsPlatformAdmin { get; set; }
    public string Status { get; set; } = "active";
    public int PermissionsVersion { get; set; } = 1;
    public DateTime? LastLogin { get; set; }
    public DateTime? DeletedAt { get; set; }
}