using Microsoft.EntityFrameworkCore;
using SchoolERP.Domain.Tenants.Entities;

namespace SchoolERP.Infrastructure.Persistence;

public class TenantDbContext : DbContext
{
    public TenantDbContext(
        DbContextOptions<TenantDbContext> options)
        : base(options)
    {
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();
}
