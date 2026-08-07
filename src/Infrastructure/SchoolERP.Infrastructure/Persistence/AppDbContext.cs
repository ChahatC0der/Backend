using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Domain.Common;
using SchoolERP.Infrastructure.Identity;

namespace SchoolERP.Infrastructure.Persistence;

public class AppDbContext : IdentityDbContext<ApplicationUser, IdentityRole<long>, long>, IApplicationDbContext
{
    private readonly ICurrentTenantService _tenantService;
    private IDbContextTransaction? _currentTransaction;

    public AppDbContext(DbContextOptions<AppDbContext> options, ICurrentTenantService tenantService)
        : base(options)
    {
        _tenantService = tenantService;
    }

    // 🔥 IApplicationDbContext Implementation
    DbSet<TEntity> IApplicationDbContext.Set<TEntity>() => base.Set<TEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // 🔥 CRITICAL: Identity tables configure karne ke liye base call karo
        base.OnModelCreating(modelBuilder);

        // ==========================================================
        // 🔥 MAP EXISTING DDL TABLES TO IDENTITY
        // ==========================================================

        // 1. Users Table (Custom ApplicationUser)
        modelBuilder.Entity<ApplicationUser>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(e => e.Id);

            // Identity base columns (yeh DDL mein honge ya migration add karega)
            entity.Property(e => e.Id).HasColumnName("Id");
            entity.Property(e => e.Email).HasColumnName("Email");
            entity.Property(e => e.PasswordHash).HasColumnName("PasswordHash");
            entity.Property(e => e.NormalizedEmail).HasColumnName("NormalizedEmail");
            entity.Property(e => e.ConcurrencyStamp).HasColumnName("ConcurrencyStamp");
            entity.Property(e => e.SecurityStamp).HasColumnName("SecurityStamp");

            // Custom columns (jo DDL mein already hain)
            entity.Property(e => e.TenantId).HasColumnName("TenantId");
            entity.Property(e => e.BranchId).HasColumnName("BranchId");
            entity.Property(e => e.Name).HasColumnName("Name");
            entity.Property(e => e.Phone).HasColumnName("Phone");
            entity.Property(e => e.IsPlatformAdmin).HasColumnName("IsPlatformAdmin");
            entity.Property(e => e.Status).HasColumnName("Status");
            entity.Property(e => e.PermissionsVersion).HasColumnName("PermissionsVersion");
            entity.Property(e => e.LastLogin).HasColumnName("LastLogin");
            entity.Property(e => e.DeletedAt).HasColumnName("DeletedAt");
        });

        // 2. Roles Table (IdentityRole)
        modelBuilder.Entity<IdentityRole<long>>(entity =>
        {
            entity.ToTable("Roles");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("Id");
            entity.Property(e => e.Name).HasColumnName("Name");
            entity.Property(e => e.NormalizedName).HasColumnName("NormalizedName");
        });

        // 3. UserRoles Table (Many-to-Many)
        modelBuilder.Entity<IdentityUserRole<long>>(entity =>
        {
            entity.ToTable("UserRoles");
            entity.HasKey(e => new { e.UserId, e.RoleId });
            entity.Property(e => e.UserId).HasColumnName("UserId");
            entity.Property(e => e.RoleId).HasColumnName("RoleId");
        });

        // 4. RolePermissions (IdentityRoleClaim) — Phase 5 mein permissions yahan store hongi
        modelBuilder.Entity<IdentityRoleClaim<long>>(entity =>
        {
            entity.ToTable("RolePermissions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ClaimType).HasColumnName("PermissionKey");
            entity.Property(e => e.ClaimValue).HasColumnName("ClaimValue");
        });

        // 5. UserPermissions (IdentityUserClaim) — Extra granular permissions
        modelBuilder.Entity<IdentityUserClaim<long>>(entity =>
        {
            entity.ToTable("UserPermissions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ClaimType).HasColumnName("PermissionKey");
            entity.Property(e => e.ClaimValue).HasColumnName("ClaimValue");
        });

        // ==========================================================
        // 🔥 MULTI-TENANCY: Global Query Filters
        // ==========================================================
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = System.Linq.Expressions.Expression.Parameter(entityType.ClrType, "e");
                var property = System.Linq.Expressions.Expression.Property(parameter, "TenantId");
                var tenantId = System.Linq.Expressions.Expression.Constant(_tenantService.GetTenantId());
                var equals = System.Linq.Expressions.Expression.Equal(property, tenantId);
                var lambda = System.Linq.Expressions.Expression.Lambda(equals, parameter);
                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
            }
        }
    }

    // 🔥 SaveChanges Override (Auto TenantId + Audit)
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries<BaseEntity>();
        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.Id = Guid.NewGuid();
                entry.Entity.TenantId = _tenantService.GetTenantId();
                entry.Entity.CreatedAt = DateTime.UtcNow;
            }
            if (entry.State == EntityState.Modified)
                entry.Entity.UpdatedAt = DateTime.UtcNow;
        }
        return await base.SaveChangesAsync(cancellationToken);
    }

    // 🔥 Transaction Methods (IApplicationDbContext)
    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction != null) return;
        _currentTransaction = await Database.BeginTransactionAsync(cancellationToken);
    }
    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction == null) return;
        await _currentTransaction.CommitAsync(cancellationToken);
        await _currentTransaction.DisposeAsync();
        _currentTransaction = null;
    }
    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction == null) return;
        await _currentTransaction.RollbackAsync(cancellationToken);
        await _currentTransaction.DisposeAsync();
        _currentTransaction = null;
    }
    public bool HasActiveTransaction => _currentTransaction != null;
}