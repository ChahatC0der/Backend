using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Extensions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SchoolERP.API.Middleware;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.Behaviors;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Infrastructure.MultiTenancy;   // 👈 Naya namespace
using SchoolERP.Infrastructure.Persistence;
using SchoolERP.Infrastructure.Services;
using SchoolERP.Infrastructure.Data;
using Microsoft.Extensions.Configuration;
using Finbuckle.MultiTenant.AspNetCore.Extensions;

var builder = WebApplication.CreateBuilder(args);

// ==========================================================
// 🔥 STEP 1: FINBUCKLE MULTI-TENANCY REGISTRATION (SERVICES)
// ==========================================================
builder.Services.AddMultiTenant<AppTenantInfo>()
    //.WithHostStrategy() // Subdomain se tenant detect karega (e.g., school1.localhost)
    //.WithHeaderStrategy() // Ya Header se (X-Tenant-Id)
    .WithInMemoryStore(options =>
    {
        options.Tenants.Add(new AppTenantInfo
        {
            Id = "11111111-1111-1111-1111-111111111111",
            Identifier = "school1",
            Name = "ABC High School"
        });
        options.Tenants.Add(new AppTenantInfo
        {
            Id = "22222222-2222-2222-2222-222222222222",
            Identifier = "school2",
            Name = "XYZ Public School"
        });
    }); // Phase 1: Static tenants (Baad mein DB store se replace karenge)

// ==========================================================
// 🔥 STEP 2: REGISTER DEPENDENCIES
// ==========================================================
// 1. DbContext (IApplicationDbContext ko AppDbContext se bind karo)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<AppDbContext>());

// 2. Current Tenant Service (Finbuckle ke accessor par based)
builder.Services.AddScoped<ICurrentTenantService, CurrentTenantService>();

// 3. Dapper Factory
//builder.Services.AddScoped<IDbConnectionFactory, SqlConnectionFactory>();
// 🔥 DAPPER GENERIC REPOSITORY REGISTRATION (Open Generic)
// ==========================================================
builder.Services.AddScoped(typeof(IDapperRepository<>), typeof(DapperRepository<>));

// 🔥 Dapper Connection Factory (Already registered, but ensure Scoped)
builder.Services.AddScoped<IDbConnectionFactory, SqlConnectionFactory>();

// 🔥 Current Tenant Service (Required for Dapper Tenant Injection)
builder.Services.AddScoped<ICurrentTenantService, CurrentTenantService>();

// 4. MediatR & Behaviors
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(ICommand<>).Assembly);
    cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
    cfg.AddOpenBehavior(typeof(PerformanceBehavior<,>));
    cfg.AddOpenBehavior(typeof(TransactionBehavior<,>));
});

// 5. FluentValidation
builder.Services.AddValidatorsFromAssembly(typeof(ICommand<>).Assembly);

// 6. Controllers
builder.Services.AddControllers();

// 7. Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ==========================================================
// 🔥 STEP 3: MIDDLEWARE PIPELINE
// ==========================================================

// 1. Global Exception Handler (Sabse Pehle)
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

// 2. 🔥 FINBUCKLE MULTI-TENANCY (Activate the middleware)
app.UseMultiTenant(); // Yeh ab kaam karega kyunki services register ho chuki hain

// 3. Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 4. Auth & Routing
app.UseAuthorization();
app.MapControllers();

app.Run();