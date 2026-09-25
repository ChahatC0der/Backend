using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SchoolERP.API;
using SchoolERP.Application.Common.Interfaces;
using System.Security.Claims;

namespace SchoolERP.IntegrationTests.Features.AI.Confirmation;

public sealed class AiConfirmationApiFactory
    : WebApplicationFactory<Program>
{
    public Guid TenantId { get; } =
        Guid.NewGuid();

    protected override void ConfigureWebHost(
        IWebHostBuilder builder)
    {
        builder.ConfigureServices(
            services =>
            {
                services.RemoveAll<
                    ICurrentTenantService>();

                services.AddScoped<
                    ICurrentTenantService>(
                    _ =>
                        new TestCurrentTenantService(
                            TenantId));

                services.AddAuthentication(
                    options =>
                    {
                        options.DefaultAuthenticateScheme =
                            TestAuthenticationDefaults
                                .AuthenticationScheme;

                        options.DefaultChallengeScheme =
                            TestAuthenticationDefaults
                                .AuthenticationScheme;

                        options.DefaultScheme =
                            TestAuthenticationDefaults
                                .AuthenticationScheme;
                    })
                    .AddScheme<
                        AuthenticationSchemeOptions,
                        TestAuthenticationHandler>(
                        TestAuthenticationDefaults
                            .AuthenticationScheme,
                        _ =>
                        {
                        });
            });
    }

    private sealed class TestCurrentTenantService
        : ICurrentTenantService
    {
        private readonly Guid _tenantId;

        public TestCurrentTenantService(
            Guid tenantId)
        {
            _tenantId =
                tenantId;
        }

        public Guid GetTenantId()
            => _tenantId;

        public string GetTenantName()
            => "Integration Test Tenant";

        public string GetTenantIdentifier()
            => "integration-test-tenant";
    }

    private static class TestAuthenticationDefaults
    {
        public const string AuthenticationScheme =
            "IntegrationTest";
    }

    private sealed class TestAuthenticationHandler
        : AuthenticationHandler<
            AuthenticationSchemeOptions>
    {
        public TestAuthenticationHandler(
            IOptionsMonitor<
                AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            System.Text.Encodings.Web.UrlEncoder encoder)
            : base(
                options,
                logger,
                encoder)
        {
        }

        protected override Task<
            AuthenticateResult>
            HandleAuthenticateAsync()
        {
            var userIdHeader =
                Request.Headers[
                    "X-Test-UserId"]
                    .FirstOrDefault();

            if (!Guid.TryParse(
                    userIdHeader,
                    out var userId))
            {
                return Task.FromResult(
                    AuthenticateResult.NoResult());
            }

            var claims =
                new[]
                {
                    new Claim(
                        ClaimTypes.NameIdentifier,
                        userId.ToString())
                };

            var identity =
                new ClaimsIdentity(
                    claims,
                    Scheme.Name);

            var principal =
                new ClaimsPrincipal(
                    identity);

            var ticket =
                new AuthenticationTicket(
                    principal,
                    Scheme.Name);

            return Task.FromResult(
                AuthenticateResult.Success(
                    ticket));
        }
    }
}