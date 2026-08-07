using Microsoft.Extensions.Caching.Memory;
using SchoolERP.Application.Common.Interfaces;

namespace SchoolERP.Infrastructure.Services;

public class MemoryCacheService : ICacheService
{
    private readonly IMemoryCache _cache;
    private readonly ICurrentTenantService _tenantService;

    public MemoryCacheService(IMemoryCache cache, ICurrentTenantService tenantService)
    {
        _cache = cache;
        _tenantService = tenantService;
    }

    // 🔥 CRITICAL: Har cache key mein TenantId prefix add karo
    private string GetTenantAwareKey(string key)
    {
        var tenantId = _tenantService.GetTenantId();
        // Agar tenant system hai (Super Admin) toh "system:" prefix, warna "tenant_{id}:"
        return tenantId != Guid.Empty ? $"tenant_{tenantId}:{key}" : $"system:{key}";
    }

    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var tenantKey = GetTenantAwareKey(key);
        _cache.TryGetValue(tenantKey, out T? value);
        return Task.FromResult(value);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken cancellationToken = default)
    {
        var tenantKey = GetTenantAwareKey(key);
        var options = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(expiry ?? TimeSpan.FromMinutes(10));

        _cache.Set(tenantKey, value, options);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        var tenantKey = GetTenantAwareKey(key);
        _cache.Remove(tenantKey);
        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        var tenantKey = GetTenantAwareKey(key);
        return Task.FromResult(_cache.TryGetValue(tenantKey, out _));
    }
}