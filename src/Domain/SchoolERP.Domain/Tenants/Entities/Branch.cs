using SchoolERP.Domain.Common;

namespace SchoolERP.Domain.Tenants.Entities;

public class Branch : GuidAuditableEntity, IMustHaveTenant
{
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? ContactPerson { get; set; }
    public bool IsDefault { get; set; }
    public string Status { get; set; } = "active";
    public string? Settings { get; set; }

    public Tenant? Tenant { get; set; }
}