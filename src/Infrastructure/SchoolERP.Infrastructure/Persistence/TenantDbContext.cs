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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply entity configurations in this assembly so TenantConfiguration is applied
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TenantDbContext).Assembly);
    }
}
