using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SchoolERP.Domain.Tenants.Entities;
using SchoolERP.Infrastructure.Persistence;

namespace SchoolERP.Infrastructure.MultiTenancy;

public class TenantStore : IMultiTenantStore<AppTenantInfo>
{
    private readonly TenantDbContext _db;
    private readonly ILogger<TenantStore> _logger;


    public TenantStore(TenantDbContext db, ILogger<TenantStore> logger)
    {
        _db = db;
        _logger = logger;

    }

    public async Task<AppTenantInfo?> GetByIdentifierAsync(string identifier)
    {
        Console.WriteLine($"🔥 Host Tenant Identifier = [{identifier}]");

        if (string.IsNullOrWhiteSpace(identifier))
            return null;

        var tenant = await _db.Tenants
     .AsNoTracking()
     .FirstOrDefaultAsync(
         x => x.Subdomain == identifier ||
              x.Id.ToString() == identifier);

        Console.WriteLine(
            tenant == null
                ? "❌ Tenant NOT FOUND"
                : $"✅ Tenant FOUND: Id={tenant.Id}, Code={tenant.Code}, Name={tenant.Name}");

        if (tenant == null)
            return null;

        return MapTenant(tenant);
    }

    public async Task<AppTenantInfo?> GetAsync(string id)
    {
        if (!Guid.TryParse(id, out var tenantId))
            return null;

        var tenant = await _db.Tenants
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == tenantId);

        if (tenant == null)
            return null;

        return MapTenant(tenant);
    }
    public async Task<IEnumerable<AppTenantInfo>> GetAllAsync(
    int pageIndex,
    int pageSize)
    {
        var tenants = await _db.Tenants
            .AsNoTracking()
            .OrderBy(x => x.Id)
            .Skip(pageIndex * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return tenants.Select(MapTenant);
    }


    public async Task<IEnumerable<AppTenantInfo>> GetAllAsync()
    {
        var tenants = await _db.Tenants
            .AsNoTracking()
            .ToListAsync();

        return tenants.Select(MapTenant);
    }

    public Task<bool> AddAsync(AppTenantInfo tenantInfo)
    {
        // Adding a tenant via the Finbuckle store is not supported by this implementation.
        // Tenant creation should use the application's Tenant CRUD API which ensures
        // all required business fields and validation are enforced.
        Console.WriteLine("TenantStore.AddAsync called — operation not supported. Use Tenant CRUD API.");
        return Task.FromResult(false);
    }

    public Task<bool> UpdateAsync(AppTenantInfo tenantInfo)
    {
        // Updating via Finbuckle store is not supported. Use Tenant CRUD API to update business entity.
        Console.WriteLine("TenantStore.UpdateAsync called — operation not supported. Use Tenant CRUD API.");
        return Task.FromResult(false);
    }

    public Task<bool> RemoveAsync(string identifier)
    {
        // Removing tenants via the Finbuckle store is not supported here.
        Console.WriteLine("TenantStore.RemoveAsync called — operation not supported. Use Tenant CRUD API.");
        return Task.FromResult(false);
    }

    private static AppTenantInfo MapTenant(Tenant tenant)
    {
        return new AppTenantInfo
        {
            Id = Convert.ToString(tenant.Id),
            Identifier = tenant.Code,
            Name = tenant.Name,
            ConnectionString = null,

            // Apne actual columns ke according:
            //IsActive = tenant.IsActive,
            //Plan = tenant.Plan
        };
    }
   

}
