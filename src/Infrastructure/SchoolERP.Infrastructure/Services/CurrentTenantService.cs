using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Abstractions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Infrastructure.MultiTenancy; // 👈 NAYA NAMESPACE

namespace SchoolERP.Infrastructure.Services;

public class CurrentTenantService : ICurrentTenantService
{
    private readonly IMultiTenantContextAccessor<AppTenantInfo> _tenantAccessor;

    public CurrentTenantService(IMultiTenantContextAccessor<AppTenantInfo> tenantAccessor)
        => _tenantAccessor = tenantAccessor;

    public Guid GetTenantId()
    {
        var tenantInfo = _tenantAccessor.MultiTenantContext?.TenantInfo;
        if (tenantInfo == null || string.IsNullOrEmpty(tenantInfo.Id))
            return Guid.Empty;
        return Guid.Parse(tenantInfo.Id);
    }

    public string GetTenantName()
        => _tenantAccessor.MultiTenantContext?.TenantInfo?.Name ?? "System";

    public string GetTenantIdentifier()
        => _tenantAccessor.MultiTenantContext?.TenantInfo?.Identifier ?? string.Empty;
}