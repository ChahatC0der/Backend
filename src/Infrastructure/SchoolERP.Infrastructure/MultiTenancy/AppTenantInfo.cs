using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Abstractions;

namespace SchoolERP.Infrastructure.MultiTenancy;

public class AppTenantInfo : ITenantInfo
{
    public string Id { get; set; } = string.Empty;
    public string Identifier { get; set; } = string.Empty; // Subdomain (e.g., "school1")
    public string Name { get; set; } = string.Empty;
    public string? ConnectionString { get; set; } = null;  // NULL = Shared DB (Row-level isolation)
}