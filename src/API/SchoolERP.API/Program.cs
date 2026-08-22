using Finbuckle.MultiTenant.AspNetCore.Extensions;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Scalar.AspNetCore;
using SchoolERP.API;
using SchoolERP.API.Middleware;
using SchoolERP.Application;
using SchoolERP.Infrastructure;
using SchoolERP.Infrastructure.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// ==========================================================
// 🔥 1. SERILOG (Logging Provider)
// ==========================================================
builder.Host.UseSerilog((context, config) =>
{
    config.ReadFrom.Configuration(context.Configuration);
});

// ==========================================================
// 🔥 2. REGISTER ALL LAYERS (CLEAN SEPARATION)
// ==========================================================
builder.Services
    .AddApplication()        // MediatR + FluentValidation
    .AddInfrastructure(builder.Configuration) // EF, Identity, Dapper, MultiTenant, Cache
    .AddApi(builder.Configuration);          // JWT, RBAC, OpenTelemetry, HealthChecks, Swagger

// 🔥 3. 🔥🔥 YAHAN ADD KARO 🔥🔥
builder.Services.AddHostedService<DatabaseHealthCheckService>(); // 👈 YE LINE

var app = builder.Build();

// ==========================================================
// 🔥 3. MIDDLEWARE PIPELINE
// ==========================================================
app.UseSerilogRequestLogging();      // HTTP Logging
app.UseMiddleware<GlobalExceptionHandlingMiddleware>(); // Exception Handler
app.UseMultiTenant();                // Multi-Tenancy

// Swagger + Scalar UI (Development)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options
            .WithTitle("SchoolERP API")
            .WithTheme(ScalarTheme.Purple)
            .WithPreferredScheme("Bearer");
    });
}

app.UseAuthentication();
app.UseAuthorization();

// Health Check
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapControllers();
app.Run();