using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Domain.Common;
using SchoolERP.Domain.Rbac.Entities;
using SchoolERP.Domain.Tenants.Entities;
using SchoolERP.Infrastructure.Identity;
using SchoolERP.Infrastructure.Persistence.Configurations; // 👈 NAYA NAMESPACE

namespace SchoolERP.Infrastructure.Persistence;

public class AppDbContext : IdentityDbContext<ApplicationUser, IdentityRole<long>, long>, IApplicationDbContext
{
    private readonly ICurrentTenantService _tenantService;
    private IDbContextTransaction? _currentTransaction;

    private Guid CurrentTenantId => _tenantService.GetTenantId();



    public AppDbContext(DbContextOptions<AppDbContext> options, ICurrentTenantService tenantService)
        : base(options)
    {
        _tenantService = tenantService;
    }

    // 🔥 Business tables
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Branch> Branches => Set<Branch>(); // 👈 Uncomment karo

    public DbSet<Module> Modules => Set<Module>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<UserToken> UserTokens => Set<UserToken>();
    public DbSet<BulkRoleJob> BulkRoleJobs => Set<BulkRoleJob>();
    public DbSet<RbacAuditLog> RbacAuditLogs => Set<RbacAuditLog>();

    DbSet<TEntity> IApplicationDbContext.Set<TEntity>() => base.Set<TEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ---- Identity Mapping (Pehle jaisa) ----
        //modelBuilder.Entity<ApplicationUser>(entity =>
        //{
        //    entity.ToTable("Users");
        //    entity.Property(e => e.TenantId).HasColumnName("TenantId");
        //    entity.Property(e => e.BranchId).HasColumnName("BranchId");
        //    entity.Property(e => e.Name).HasColumnName("Name");
        //    entity.Property(e => e.Phone).HasColumnName("Phone");
        //    entity.Property(e => e.IsPlatformAdmin).HasColumnName("IsPlatformAdmin");
        //    entity.Property(e => e.Status).HasColumnName("Status");
        //    entity.Property(e => e.PermissionsVersion).HasColumnName("PermissionsVersion");
        //    entity.Property(e => e.LastLogin).HasColumnName("LastLogin");
        //    entity.Property(e => e.DeletedAt).HasColumnName("DeletedAt");
        //});

        //modelBuilder.Entity<IdentityRole<long>>(entity => entity.ToTable("Roles"));
        //modelBuilder.Entity<IdentityUserRole<long>>(entity => entity.ToTable("UserRoles"));
        //modelBuilder.Entity<IdentityRoleClaim<long>>(entity =>
        //{
        //    entity.ToTable("RolePermissions");
        //    entity.Property(e => e.ClaimType).HasColumnName("PermissionKey");
        //});
        //modelBuilder.Entity<IdentityUserClaim<long>>(entity =>
        //{
        //    entity.ToTable("UserPermissions");
        //    entity.Property(e => e.ClaimType).HasColumnName("PermissionKey");
        //});

        // ---- 🔥 ALL ENTITY CONFIGURATIONS LOADED FROM SEPARATE FILES ----
        // Ab Tenant, Branch, MasterCategory, MasterItem etc. ki alag configuration files se apply hongi.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

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
        if (CurrentTenantId == Guid.Empty)
        {
            // ✅ Optional: Log karo for debugging
            Console.WriteLine($"⚠️ WARNING: CurrentTenantId is empty! Check Finbuckle configuration.");

            // ✅ Throw meaningful error instead of FK constraint error
            throw new InvalidOperationException(
                "Tenant context not resolved. Please ensure X-Tenant-Id header or query parameter is provided.");
        }
        // 🔥 Auto-set TenantId
        foreach (var entry in ChangeTracker.Entries<IMustHaveTenant>())
        {
            if (entry.State == EntityState.Added)
                entry.Entity.TenantId = CurrentTenantId;
        }

        // 🔥 Auto-set CreatedAt & UpdatedAt
        foreach (var entry in ChangeTracker.Entries<IAuditableTimestamps>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTime.UtcNow;
                entry.Entity.UpdatedAt = DateTime.UtcNow; // ✅ CREATE par bhi set
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow; // ✅ UPDATE par set
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }

    // ---- Transaction Methods ----
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