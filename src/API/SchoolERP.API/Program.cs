using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.AspNetCore.Extensions;
using Finbuckle.MultiTenant.Extensions;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SchoolERP.API.Middleware;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.Behaviors;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Infrastructure.Data;
using SchoolERP.Infrastructure.Identity;
using SchoolERP.Infrastructure.MultiTenancy;
using SchoolERP.Infrastructure.Persistence;
using SchoolERP.Infrastructure.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ==========================================================
// 🔥 1. MULTI-TENANCY (Finbuckle)
// ==========================================================
builder.Services.AddMultiTenant<AppTenantInfo>()
    .WithHostStrategy()          // 👈 UNCOMMENTED: school1.localhost se tenant detect
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
// 🔥 2. IDENTITY (User Store)
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
// 🔥 3. JWT AUTHENTICATION (Token Read Karne Ke Liye)
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
// 🔥 4. DEPENDENCY INJECTION (Clean Architecture)
// ==========================================================

// DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));
builder.Services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<AppDbContext>());

// Multi-Tenancy Service (Singleton/Scoped)
builder.Services.AddScoped<ICurrentTenantService, CurrentTenantService>(); // 👈 SINGLE REGISTRATION

// Dapper
builder.Services.AddScoped<IDbConnectionFactory, SqlConnectionFactory>();
builder.Services.AddScoped(typeof(IDapperRepository<>), typeof(DapperRepository<>));

// MediatR + Behaviors
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(ICommand<>).Assembly);
    cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
    cfg.AddOpenBehavior(typeof(PerformanceBehavior<,>));
    cfg.AddOpenBehavior(typeof(TransactionBehavior<,>));
});

// FluentValidation
builder.Services.AddValidatorsFromAssembly(typeof(ICommand<>).Assembly);

// Controllers & Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ==========================================================
// 🔥 5. MIDDLEWARE PIPELINE (ORDER IS CRITICAL!)
// ==========================================================

// 1. Global Exception Handler (Sabse Pehle)
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

// 2. Multi-Tenancy (Finbuckle)
app.UseMultiTenant();

// 3. Swagger (Development)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 4. 🔥 AUTHENTICATION & AUTHORIZATION (YEH MISSING THA)
app.UseAuthentication(); // 👈 JWT token ko read karega
app.UseAuthorization();  // 👈 [Authorize] attribute enforce karega

// 5. Routing
app.MapControllers();

app.Run();