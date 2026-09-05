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
        var id = _tenantAccessor.MultiTenantContext?.TenantInfo?.Id;

        return Guid.TryParse(id, out var tenantId)
            ? tenantId
            : Guid.Empty;
    }
    public Guid GetBranchId()
    {
        var id = _tenantAccessor.MultiTenantContext?.TenantInfo?.Id;

        return Guid.TryParse(id, out var tenantId)
            ? tenantId
            : Guid.Empty;
    }

    public string GetTenantName()
        => _tenantAccessor.MultiTenantContext?.TenantInfo?.Name ?? "System";

    public string GetTenantIdentifier()
        => _tenantAccessor.MultiTenantContext?.TenantInfo?.Identifier ?? string.Empty;
}