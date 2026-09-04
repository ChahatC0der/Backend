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

app.UseSerilogRequestLogging();
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
app.UseMultiTenant();

// 🔥 Add UseRouting here
app.UseRouting();

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

app.UseMultiTenant();

app.Use(async (context, next) =>
{
    Console.WriteLine("===== BEFORE AUTH =====");
    Console.WriteLine($"Scheme: {context.User.Identity?.AuthenticationType}");

    await next();

    Console.WriteLine("===== AFTER AUTH =====");
    Console.WriteLine($"Authenticated: {context.User.Identity?.IsAuthenticated}");
    Console.WriteLine($"AuthType: {context.User.Identity?.AuthenticationType}");
});

app.UseAuthentication();

app.Use(async (context, next) =>
{
    Console.WriteLine("===== AFTER AUTHENTICATION MIDDLEWARE =====");
    Console.WriteLine($"Authenticated: {context.User.Identity?.IsAuthenticated}");
    Console.WriteLine($"AuthType: {context.User.Identity?.AuthenticationType}");

    await next();
});

app.UseAuthorization();

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapControllers();
app.Run();