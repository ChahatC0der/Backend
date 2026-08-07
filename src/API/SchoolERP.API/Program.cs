using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.AspNetCore.Extensions;
using Finbuckle.MultiTenant.Extensions;
using FluentValidation;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Scalar.AspNetCore;
using SchoolERP.API.Authorization;
using SchoolERP.API.Middleware;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.Behaviors;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Infrastructure.Data;
using SchoolERP.Infrastructure.Identity;
using SchoolERP.Infrastructure.MultiTenancy;
using SchoolERP.Infrastructure.Persistence;
using SchoolERP.Infrastructure.Services;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ==========================================================
// 🔥 1. SERILOG SETUP (Automatic Logging - Phase 6)
// ==========================================================
builder.Host.UseSerilog((context, config) =>
{
    config.ReadFrom.Configuration(context.Configuration);
});

// Ye add karo, jahan baaki services register ho rahe hain
builder.Services.AddOpenApi(
//    options =>
//{
//    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
//}
);
// ==========================================================
// 🔥 2. MULTI-TENANCY (Finbuckle - Phase 3)
// ==========================================================
builder.Services.AddMultiTenant<AppTenantInfo>()
    .WithHostStrategy()          // school1.localhost se tenant detect
    .WithHeaderStrategy()        // Ya Header se (X-Tenant-Id)
    .WithInMemoryStore(options =>
    {
        options.Tenants = new List<AppTenantInfo>
        {
            new AppTenantInfo
            {
                Id = "11111111-1111-1111-1111-111111111111",
                Identifier = "school1",
                Name = "ABC High School"
            },
            new AppTenantInfo
            {
                Id = "22222222-2222-2222-2222-222222222222",
                Identifier = "school2",
                Name = "XYZ Public School"
            }
        };
    });

// ==========================================================
// 🔥 3. IDENTITY (User Store - Phase 4)
// ==========================================================
builder.Services.AddIdentity<ApplicationUser, IdentityRole<long>>(options =>
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
// 🔥 4. JWT AUTHENTICATION (Phase 4)
// ==========================================================
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Secret"]!))
    };
});

// ==========================================================
// 🔥 5. DEPENDENCY INJECTION (Clean Architecture)
// ==========================================================

// DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));
builder.Services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<AppDbContext>());

// RBAC (Phase 5)
builder.Services.AddScoped<IAuthorizationHandler, PermissionHandler>();
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();

// Multi-Tenancy Service
builder.Services.AddScoped<ICurrentTenantService, CurrentTenantService>();

// Dapper (Phase 3 Ext)
builder.Services.AddScoped<IDbConnectionFactory, SqlConnectionFactory>();
builder.Services.AddScoped(typeof(IDapperRepository<>), typeof(DapperRepository<>));

// 🔥 CACHING (Phase 7 - WITH TENANT AWARENESS)
builder.Services.AddMemoryCache();
builder.Services.AddScoped<ICacheService, MemoryCacheService>(); // 👈 FIXED VERSION

// MediatR + Behaviors (Phase 1)
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(ICommand<>).Assembly);
    cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
    cfg.AddOpenBehavior(typeof(PerformanceBehavior<,>));
    cfg.AddOpenBehavior(typeof(TransactionBehavior<,>));
});

// FluentValidation (Phase 1)
builder.Services.AddValidatorsFromAssembly(typeof(ICommand<>).Assembly);

// 🔥 OPEN TELEMETRY (TRACING - Phase 6)
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing =>
    {
        tracing
            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("SchoolERP.API"))
            .AddAspNetCoreInstrumentation()     // Automatic HTTP Tracing
            .AddSqlClientInstrumentation()      // Automatic SQL Tracing
            .AddOtlpExporter();                 // Jaeger / OTLP Collector
    });

// 🔥 HEALTH CHECKS (Phase 6)
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>()
    .AddSqlServer(
        builder.Configuration.GetConnectionString("Default"),
        name: "SQL Server",
        tags: new[] { "database", "sql" });

// Controllers & API
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ==========================================================
// 🔥 6. MIDDLEWARE PIPELINE (ORDER IS CRITICAL!)
// ==========================================================

// 1. 🔥 SERILOG REQUEST LOGGING (Automatic HTTP Logs - Phase 6)
app.UseSerilogRequestLogging();

// 2. Global Exception Handler (Phase 2)
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

// 3. Multi-Tenancy (Finbuckle - Phase 3)
app.UseMultiTenant();

// 4. Swagger / Scalar UI (Development)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    app.MapOpenApi();

    // 🔥 SCALAR UI (Modern API Docs - Phase 2.5)
    app.MapScalarApiReference(options =>
    {
        options
            .WithTitle("SchoolERP API")
            .WithTheme(ScalarTheme.Purple)
            .WithPreferredScheme("Bearer");
    });
}

// 5. Authentication & Authorization (Phase 4 & 5)
app.UseAuthentication();
app.UseAuthorization();

// 6. 🔥 HEALTH CHECK ENDPOINT (Phase 6)
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

// 7. Routing
app.MapControllers();

app.Run();