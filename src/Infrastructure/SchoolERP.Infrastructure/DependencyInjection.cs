using Finbuckle.MultiTenant.AspNetCore.Extensions;
using Finbuckle.MultiTenant.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.AI.Guardrails;
using SchoolERP.Application.Features.AI.Services;
using SchoolERP.Application.Features.AI.Tools;
using SchoolERP.Infrastructure.AI;
using SchoolERP.Infrastructure.Data;
using SchoolERP.Infrastructure.Identity;
using SchoolERP.Infrastructure.MultiTenancy;
using SchoolERP.Infrastructure.Persistence;
using SchoolERP.Infrastructure.Services;
using SchoolERP.Infrastructure.Services.AI;
using SchoolERP.Infrastructure.Services.AI.Guardrails;
using SchoolERP.Infrastructure.Services.AI.Providers;
using SchoolERP.Infrastructure.Services.AI.Tools;


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
        services.AddScoped<ICurrentBranchService, CurrentBranchService>();
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

        services.AddScoped<IStaffIdGenerator, StaffIdGenerator>();
        services.AddScoped<IEnrollmentIdGenerator, EnrollmentIdGenerator>();
        services.AddScoped<IExcelExportService, ClosedXmlExportService>();

        services.AddScoped<IAiGateway, AiGateway>();

        services.AddSingleton<IAiToolRegistry>(sp =>
        {
            var registry =
                new InMemoryAiToolRegistry();

            foreach (var tool in AiToolDefinitions.GetAll())
            {
                registry.Register(tool);
            }

            return registry;
        });

        services.AddSingleton<
    IAiToolSchemaValidator,
    JsonSchemaAiToolSchemaValidator>();

        services.AddHttpClient<OpenRouterAiProvider>(client =>
        {
            client.BaseAddress = new Uri(
                configuration["AI:BaseUrl"]
                ?? "https://openrouter.ai/api/v1/");
        });

        services.AddScoped<IAiProvider>(
            sp => sp.GetRequiredService<OpenRouterAiProvider>());
        services.AddScoped<IAiPermissionProvider, HttpAiPermissionProvider>();
        services.AddScoped<IAiBranchContextProvider, HttpAiBranchContextProvider>();
        services.AddScoped<IAiExecutionContextAccessor, HttpAiExecutionContextAccessor>();

        services.AddScoped<AiToolScopeAuthorizationService>();
        services.AddScoped<AiToolAuthorizationService>();

        services.AddScoped<IAiInputGuardrail, AiMessageTrustBoundaryGuardrail>();
        services.AddScoped<IAiInputGuardrail, PromptInjectionGuardrail>();
        services.AddScoped<IAiInputGuardrailService, AiInputGuardrailService>();

        services.AddScoped<IAiOutputGuardrail, AiSensitiveOutputGuardrail>();
        services.AddScoped<IAiOutputGuardrailService, AiOutputGuardrailService>();

        services.AddScoped<IAiSystemPromptPolicy, AiSystemPromptPolicy>();


        return services;
    }
}