using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using SchoolERP.API.Authorization;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Infrastructure.Persistence;
using SchoolERP.Infrastructure.Services;
using System.Text;

namespace SchoolERP.API;

public static class DependencyInjection
{
    public static IServiceCollection AddApi(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ==========================================================
        // 🔥 1. AUTHENTICATION (JWT)
        // ==========================================================
        // JWT Authentication
        services.AddAuthentication(options =>
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

        ValidIssuer = configuration["Jwt:Issuer"],
        ValidAudience = configuration["Jwt:Audience"],

        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(configuration["Jwt:Secret"]!)
        )
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            Console.WriteLine("===== JWT MESSAGE RECEIVED =====");
            Console.WriteLine(
                $"Token present: {!string.IsNullOrEmpty(context.Token)}"
            );

            return Task.CompletedTask;
        },

        OnAuthenticationFailed = context =>
        {
            Console.WriteLine("===== JWT AUTH FAILED =====");
            Console.WriteLine(context.Exception.ToString());

            return Task.CompletedTask;
        },

        OnTokenValidated = context =>
        {
            Console.WriteLine("===== JWT TOKEN VALIDATED =====");
            Console.WriteLine(
                $"User: {context.Principal?.Identity?.Name}"
            );

            return Task.CompletedTask;
        }
    };
});

        services.AddAuthorization();

        // Register JWT service
        services.AddScoped<IJwtTokenService, JwtTokenService>();



        // ==========================================================
        // 🔥 2. AUTHORIZATION (RBAC - Permission Handler)
        // ==========================================================
        services.AddScoped<IAuthorizationHandler, PermissionHandler>();
        services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();

        // Add HttpContextAccessor
        services.AddHttpContextAccessor();

        // Permission service
        services.AddScoped<IPermissionService, PermissionService>();

        // ==========================================================
        // 🔥 3. OPEN TELEMETRY (Tracing)
        // ==========================================================
        services.AddOpenTelemetry()
            .WithTracing(tracing =>
            {
                tracing
                    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("SchoolERP.API"))
                    .AddAspNetCoreInstrumentation()
                    .AddSqlClientInstrumentation()
                    .AddOtlpExporter();
            });

        // ==========================================================
        // 🔥 4. HEALTH CHECKS
        // ==========================================================
        services.AddHealthChecks()
            .AddDbContextCheck<AppDbContext>()
            .AddSqlServer(
                configuration.GetConnectionString("Default"),
                name: "SQL Server",
                tags: new[] { "database", "sql" });

        // ==========================================================
        // 🔥 5. CONTROLLERS
        // ==========================================================
        services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .SelectMany(x => x.Value!.Errors)
                .Select(x => x.ErrorMessage)
                .ToList();

            var response = new
            {
                success = false,
                error = string.Join(" | ", errors)
            };

            return new BadRequestObjectResult(response);
        };
    });

        // ==========================================================
        // 🔥 6. API DOCUMENTATION (Swagger + Scalar)
        // ==========================================================
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        services.AddOpenApi();

        return services;
    }
}