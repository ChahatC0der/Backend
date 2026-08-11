using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using SchoolERP.Infrastructure.Identity;
using SchoolERP.Infrastructure.Persistence;
using SchoolERP.IntegrationTests.Fixtures;
using SchoolERP.IntegrationTests.Helpers;

namespace SchoolERP.IntegrationTests.TestBase;

public abstract class TestBase : IClassFixture<CustomWebApplicationFactory>, IClassFixture<DatabaseFixture>, IDisposable
{
    protected readonly CustomWebApplicationFactory Factory;
    protected readonly DatabaseFixture DbFixture;
    protected readonly HttpClient Client;
    protected readonly AuthHelper AuthHelper;
    protected readonly BogusDataFixture BogusData;
    protected readonly IServiceScope Scope;
    protected readonly AppDbContext DbContext;

    protected TestBase(CustomWebApplicationFactory factory, DatabaseFixture dbFixture)
    {
        Factory = factory;
        DbFixture = dbFixture;
        Client = factory.CreateClient();

        Scope = factory.Services.CreateScope();
        var serviceProvider = Scope.ServiceProvider;
        DbContext = serviceProvider.GetRequiredService<AppDbContext>();
        AuthHelper = new AuthHelper(serviceProvider);
        BogusData = new BogusDataFixture(
            serviceProvider.GetRequiredService<UserManager<ApplicationUser>>(),
            DbContext,
            Guid.Parse("11111111-1111-1111-1111-111111111111")
        );

        // 🔥 Reset database before each test
        DbFixture.ResetDatabaseAsync().Wait();
    }

    public async Task<string> GetAdminTokenAsync()
    {
        return await AuthHelper.GetTokenAsync("admin@school.com", "Admin@123");
    }

    public void Dispose()
    {
        Scope.Dispose();
        DbFixture.ResetDatabaseAsync().Wait();
    }
}