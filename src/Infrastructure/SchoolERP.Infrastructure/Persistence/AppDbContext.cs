using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using OpenTelemetry;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Domain.Common;
using SchoolERP.Domain.Tenants.Entities;
using SchoolERP.Infrastructure.Identity;

namespace SchoolERP.Infrastructure.Persistence;

public class AppDbContext : IdentityDbContext<ApplicationUser, IdentityRole<long>, long>, IApplicationDbContext
{
    private readonly ICurrentTenantService _tenantService;
    private IDbContextTransaction? _currentTransaction;

    private Guid CurrentTenantId => _tenantService.GetTenantId();
    //private Guid CurrentBranchId => _tenantService.GetBranchId();

    public AppDbContext(DbContextOptions<AppDbContext> options, ICurrentTenantService tenantService)
        : base(options)
    {
        _tenantService = tenantService;
    }

    // 🔥 Business tables
    public DbSet<Tenant> Tenants => Set<Tenant>();
    //public DbSet<Branch> Branches => Set<Branch>();

    DbSet<TEntity> IApplicationDbContext.Set<TEntity>() => base.Set<TEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ---- Identity Mapping (jaisa pehle tha) ----
        modelBuilder.Entity<ApplicationUser>(entity =>
        {
            entity.ToTable("Users");
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

        modelBuilder.Entity<IdentityRole<long>>(entity => entity.ToTable("Roles"));
        modelBuilder.Entity<IdentityUserRole<long>>(entity => entity.ToTable("UserRoles"));
        modelBuilder.Entity<IdentityRoleClaim<long>>(entity =>
        {
            entity.ToTable("RolePermissions");
            entity.Property(e => e.ClaimType).HasColumnName("PermissionKey");
        });
        modelBuilder.Entity<IdentityUserClaim<long>>(entity =>
        {
            entity.ToTable("UserPermissions");
            entity.Property(e => e.ClaimType).HasColumnName("PermissionKey");
        });

        // ---- Tenant / Branch Table Config ----
        modelBuilder.Entity<Tenant>(entity =>
        {
            entity.ToTable("Tenants");
            entity.HasIndex(e => e.Subdomain).IsUnique();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Subdomain).IsRequired().HasMaxLength(100);
        });

        //modelBuilder.Entity<Branch>(entity =>
        //{
        //    entity.ToTable("Branches");
        //    entity.HasIndex(e => new { e.TenantId, e.Code }).IsUnique();
        //    entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
        //    entity.Property(e => e.Code).IsRequired().HasMaxLength(50);
        //});

        // ---- Global Query Filters (Tenant / Branch Isolation) ----
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var clrType = entityType.ClrType;

            if (typeof(IMustHaveBranch).IsAssignableFrom(clrType))
            {
                var parameter = System.Linq.Expressions.Expression.Parameter(clrType, "e");

                var tenantProp = System.Linq.Expressions.Expression.Property(parameter, "TenantId");
                var currentTenantExpr = System.Linq.Expressions.Expression.Property(
                    System.Linq.Expressions.Expression.Constant(this), nameof(CurrentTenantId));
                var tenantEquals = System.Linq.Expressions.Expression.Equal(tenantProp, currentTenantExpr);

                //var branchProp = System.Linq.Expressions.Expression.Property(parameter, "BranchId");
                //var currentBranchExpr = System.Linq.Expressions.Expression.Property(
                //    System.Linq.Expressions.Expression.Constant(this), nameof(CurrentBranchId));
                //var branchEquals = System.Linq.Expressions.Expression.Equal(branchProp, currentBranchExpr);

               // var combined = System.Linq.Expressions.Expression.AndAlso(tenantEquals, branchEquals);
                modelBuilder.Entity(clrType).HasQueryFilter(
                    System.Linq.Expressions.Expression.Lambda(tenantEquals, parameter));
            }
            else if (typeof(IMustHaveTenant).IsAssignableFrom(clrType))
            {
                var parameter = System.Linq.Expressions.Expression.Parameter(clrType, "e");
                var tenantProp = System.Linq.Expressions.Expression.Property(parameter, "TenantId");
                var currentTenantExpr = System.Linq.Expressions.Expression.Property(
                    System.Linq.Expressions.Expression.Constant(this), nameof(CurrentTenantId));
                var equals = System.Linq.Expressions.Expression.Equal(tenantProp, currentTenantExpr);
                modelBuilder.Entity(clrType).HasQueryFilter(
                    System.Linq.Expressions.Expression.Lambda(equals, parameter));
            }
            // Tenant, Branch, AdminUserTenant → koi filter nahi (IMustHaveTenant implement hi nahi karte)
        }
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<IMustHaveTenant>())
        {
            if (entry.State == EntityState.Added)
                entry.Entity.TenantId = CurrentTenantId;
        }

        //foreach (var entry in ChangeTracker.Entries<IMustHaveBranch>())
        //{
        //    if (entry.State == EntityState.Added)
        //        entry.Entity.BranchId = CurrentBranchId;
        //}

        // 🔥 Ab dono hierarchies (long-based aur Guid-based) ke liye kaam karega
        foreach (var entry in ChangeTracker.Entries<IAuditableTimestamps>())
        {
            if (entry.State == EntityState.Added)
                entry.Entity.CreatedAt = DateTime.UtcNow;
            if (entry.State == EntityState.Modified)
                entry.Entity.UpdatedAt = DateTime.UtcNow;
        }

        return await base.SaveChangesAsync(cancellationToken);
    }

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