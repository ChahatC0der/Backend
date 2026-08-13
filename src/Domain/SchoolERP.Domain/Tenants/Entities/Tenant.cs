using SchoolERP.Domain.Common;

namespace SchoolERP.Domain.Tenants.Entities;

public class Tenant : GuidAuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string Subdomain { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public string? ContactPhone { get; set; }
    public string? Address { get; set; }
    public string Plan { get; set; } = "basic"; // free, basic, pro, enterprise
    public string Status { get; set; } = "active"; // active, suspended, trial, expired
    public string? Settings { get; set; } // JSON
    public string? CustomFieldsDef { get; set; } // JSON
    public int StudentCount { get; set; }
    public long StorageUsedMb { get; set; }
    public long ApiCallsMonth { get; set; }
}