using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Abstractions;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Domain.Tenants.Entities;
using SchoolERP.Infrastructure.Persistence;

namespace SchoolERP.Infrastructure.MultiTenancy;

public class TenantStore : IMultiTenantStore<AppTenantInfo>
{
    private readonly TenantDbContext _db;

    public TenantStore(TenantDbContext db)
    {
        _db = db;
    }

    public async Task<AppTenantInfo?> GetByIdentifierAsync(
        string identifier)
    {
        var tenantId = Guid.Parse(identifier);

        var tenant = await _db.Tenants
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Code == identifier);


        if (tenant == null)
            return null;

        return MapTenant(tenant);
    }

    public async Task<AppTenantInfo?> GetAsync(string id)
    {
        var tenantId = Guid.Parse(id);

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
        throw new NotImplementedException();
    }

    public Task<bool> UpdateAsync(AppTenantInfo tenantInfo)
    {
        throw new NotImplementedException();
    }

    public Task<bool> RemoveAsync(string identifier)
    {
        throw new NotImplementedException();
    }

    private static AppTenantInfo MapTenant(Tenant tenant)
    {
        return new AppTenantInfo
        {
            Id = Convert.ToString(tenant.Id),
            Identifier = tenant.Code,
            Name = tenant.Name,

            // Apne actual columns ke according:
            //IsActive = tenant.IsActive,
            //Plan = tenant.Plan
        };
    }
   

}
