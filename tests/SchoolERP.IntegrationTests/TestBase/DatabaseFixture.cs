using Microsoft.EntityFrameworkCore;
using Respawn;
using Respawn.Graph; // For Table type
using Microsoft.Data.SqlClient; // ✅ Use Microsoft.Data.SqlClient
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Infrastructure.Persistence;
using System.Data.Common;

namespace SchoolERP.IntegrationTests.TestBase;

public class DatabaseFixture : IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory;
    private Respawner _respawner = null!;
    private DbConnection _dbConnection = null!; // ✅ Store as DbConnection
    private string _connectionString = string.Empty;

    public DatabaseFixture(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    public async Task InitializeAsync()
    {
        _connectionString = _factory.GetConnectionString();

        // ✅ 1. Create and open the DbConnection
        _dbConnection = new SqlConnection(_connectionString);
        await _dbConnection.OpenAsync();

        // ✅ 2. Pass the open DbConnection to Respawn
        _respawner = await Respawner.CreateAsync(_dbConnection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.SqlServer,
            SchemasToInclude = new[] { "dbo" },
            TablesToIgnore = new Table[]
            {
                "__EFMigrationsHistory"
            }
        });
    }

    public async Task ResetDatabaseAsync()
    {
        // ✅ Pass the DbConnection to ResetAsync as well
        await _respawner.ResetAsync(_dbConnection);
    }

    public AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(_connectionString)
            .Options;
        return new AppDbContext(options, new TestCurrentTenantService());
    }

    public async Task DisposeAsync()
    {
        await _dbConnection.DisposeAsync();
    }
}

public class TestCurrentTenantService : ICurrentTenantService
{
    private Guid _tenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    public void SetTenant(Guid tenantId) => _tenantId = tenantId;
    public Guid GetTenantId() => _tenantId;
    public string GetTenantName() => "Test School";
    public string GetTenantIdentifier() => "school1";
}