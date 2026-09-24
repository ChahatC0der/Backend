using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SchoolERP.Application.Common.Behaviors;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.AI.Agent;
using SchoolERP.Application.Features.AI.Services;
using SchoolERP.Application.Features.AI.Tools;
using System.Reflection;


namespace SchoolERP.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        // ==========================================================
        // 🔥 MEDIATR (CQRS Pipeline)
        // ==========================================================
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());

            // 🔥 Global Behaviors (Order matters!)
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));      // 1. Log
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));   // 2. Validate
            cfg.AddOpenBehavior(typeof(PerformanceBehavior<,>));  // 3. Performance
            cfg.AddOpenBehavior(typeof(TransactionBehavior<,>));  // 4. Transaction (SaveChanges)
        });

        // ==========================================================
        // 🔥 FLUENTVALIDATION (Auto-scan all validators)
        // ==========================================================
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        services.AddScoped<AiActionValidationService>();
        services.AddScoped<IAiAgent, AiAgent>();
        services.AddScoped<IAiToolExecutor, AiToolExecutor>();
        services.AddScoped<AiToolAuthorizationService>();
        return services;
    }
}