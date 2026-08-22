using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SchoolERP.Infrastructure.Persistence;

namespace SchoolERP.Infrastructure.Services;

public class DatabaseHealthCheckService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DatabaseHealthCheckService> _logger;

    public DatabaseHealthCheckService(
        IServiceProvider serviceProvider,
        ILogger<DatabaseHealthCheckService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // 🔥 Try to open connection
            await dbContext.Database.OpenConnectionAsync(cancellationToken);
            dbContext.Database.CloseConnection();

            _logger.LogInformation("✅ Database connected successfully! (Server: {Server})",
                dbContext.Database.GetDbConnection().DataSource);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to connect to database: {Message}", ex.Message);
            // Application continue karega, but log me error dikhega
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        // Kuch nahi karna
        return Task.CompletedTask;
    }
}