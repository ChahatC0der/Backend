using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace SchoolERP.IntegrationTests.Features.AI.Confirmation;

public sealed class AiConfirmationApiTests
    : IClassFixture<AiConfirmationApiFactory>
{
    private readonly HttpClient _client;

    public AiConfirmationApiTests(
        AiConfirmationApiFactory factory)
    {
        _client =
            factory.CreateClient();
    }

    [Fact]
    public async Task Confirm_endpoint_should_require_authentication()
    {
        var response =
            await _client.PostAsJsonAsync(
                "/api/ai/confirm",
                new
                {
                    ConfirmationToken =
                        "invalid-token"
                });

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Confirm_endpoint_should_validate_empty_token()
    {
        var userId =
            Guid.NewGuid();

        using var request =
            new HttpRequestMessage(
                HttpMethod.Post,
                "/api/ai/confirm");

        request.Headers.Add(
            "X-Test-UserId",
            userId.ToString());

        request.Content =
            JsonContent.Create(
                new
                {
                    ConfirmationToken =
                        string.Empty
                });

        var response =
            await _client.SendAsync(
                request);

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Confirm_endpoint_should_reject_invalid_confirmation_token()
    {
        var userId =
            Guid.NewGuid();

        using var request =
            new HttpRequestMessage(
                HttpMethod.Post,
                "/api/ai/confirm");

        request.Headers.Add(
            "X-Test-UserId",
            userId.ToString());

        request.Content =
            JsonContent.Create(
                new
                {
                    ConfirmationToken =
                        "invalid-or-tampered-token"
                });

        var response =
            await _client.SendAsync(
                request);

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Unauthorized);
    }
}