using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.AspNetCore.Extensions;
using Finbuckle.MultiTenant.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Domain.Tenants.Entities;
using SchoolERP.Infrastructure.Data;
using SchoolERP.Infrastructure.Identity;
using SchoolERP.Infrastructure.MultiTenancy;
using SchoolERP.Infrastructure.Persistence;
using SchoolERP.Infrastructure.Services;

namespace SchoolERP.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ==========================================================
        // 🔥 1. DATABASE (EF Core)
        // ==========================================================
        services.AddDbContext<TenantDbContext>(options =>
     options.UseSqlServer(
         configuration.GetConnectionString("Default")));

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("Default")));

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<AppDbContext>());



        // ==========================================================
        // 🔥 2. IDENTITY (User Store)
        // ==========================================================
        services.AddIdentity<ApplicationUser, IdentityRole<long>>(options =>
        {
            options.Password.RequireDigit = false;
            options.Password.RequiredLength = 4;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireLowercase = false;
        })
        .AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders();

        // ==========================================================
        // 🔥 3. MULTI-TENANCY (Finbuckle)
        // ==========================================================

        services.AddScoped<TenantStore>();

        services.AddMultiTenant<AppTenantInfo>()
            .WithHostStrategy()
     .WithHeaderStrategy("TenantId")
     .WithStore(
         ServiceLifetime.Scoped,
         sp => sp.GetRequiredService<TenantStore>());



        // ==========================================================
        // 🔥 4. TENANT SERVICE (Current Context)
        // ==========================================================
        services.AddScoped<ICurrentTenantService, CurrentTenantService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        // ==========================================================
        // 🔥 5. DAPPER (Read Side)
        // ==========================================================
        services.AddScoped<IDbConnectionFactory, SqlConnectionFactory>();
        services.AddScoped(typeof(IDapperRepository<>), typeof(DapperRepository<>));

        // ==========================================================
        // 🔥 6. CACHING (Tenant-Aware)
        // ==========================================================
        services.AddMemoryCache();
        services.AddScoped<ICacheService, MemoryCacheService>();

        services.AddScoped<IExcelExportService, ClosedXmlExportService>();
        return services;
    }
}