using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.AspNetCore.Extensions;
using Finbuckle.MultiTenant.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SchoolERP.Application.Common.Interfaces;
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
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("Default")));

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
        services.AddMultiTenant<AppTenantInfo>()
            .WithHostStrategy()
            .WithHeaderStrategy()
            .WithInMemoryStore(options =>
            {
                options.Tenants = new List<AppTenantInfo>
                {
                    new() { Id = "11111111-1111-1111-1111-111111111111",
                            Identifier = "school1", Name = "ABC High School" },
                    new() { Id = "22222222-2222-2222-2222-222222222222",
                            Identifier = "school2", Name = "XYZ Public School" }
                };
            });

        // ==========================================================
        // 🔥 4. TENANT SERVICE (Current Context)
        // ==========================================================
        services.AddScoped<ICurrentTenantService, CurrentTenantService>();

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

        return services;
    }
}