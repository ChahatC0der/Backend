using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SchoolERP.Application.Features.AI.DTOs;
using SchoolERP.Application.Features.AI.Streaming;
using SchoolERP.IntegrationTests.TestBase;
using Xunit;

namespace SchoolERP.IntegrationTests.Features.AI;

public sealed class AIStreamControllerIntegrationTests :
    IClassFixture<AgentStreamWebApplicationFactory>
{
    private readonly AgentStreamWebApplicationFactory _factory;

    public AIStreamControllerIntegrationTests(
        AgentStreamWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task AgentStream_Should_Return_Confirmation_Required_And_Stop()
    {
        using var client = CreateAuthenticatedClient();

        var request = CreateRequest("confirmation-model");

        using var response = await PostAgentStreamAsync(
            client,
            request);

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        Assert.Equal(
            "text/event-stream",
            response.Content.Headers.ContentType?.MediaType);

        var body = await response.Content.ReadAsStringAsync();

        var toolProposedIndex =
            body.IndexOf(
                "event: ToolProposed",
                StringComparison.Ordinal);

        var executionStartedIndex =
            body.IndexOf(
                "event: ToolExecutionStarted",
                StringComparison.Ordinal);

        var confirmationIndex =
            body.IndexOf(
                "event: ConfirmationRequired",
                StringComparison.Ordinal);

        Assert.True(toolProposedIndex >= 0);

        Assert.True(
            executionStartedIndex > toolProposedIndex);

        Assert.True(
            confirmationIndex > executionStartedIndex);

        Assert.DoesNotContain(
            "event: ToolExecutionCompleted",
            body);

        Assert.DoesNotContain(
            "event: MessageCompleted",
            body);
    }

    [Fact]
    public async Task AgentStream_Should_Return_Completed_After_Successful_Tool_Execution()
    {
        using var client = CreateAuthenticatedClient();

        var request = CreateRequest("success-model");

        using var response = await PostAgentStreamAsync(
            client,
            request);

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();

        var toolProposedIndex =
            body.IndexOf(
                "event: ToolProposed",
                StringComparison.Ordinal);

        var executionStartedIndex =
            body.IndexOf(
                "event: ToolExecutionStarted",
                StringComparison.Ordinal);

        var executionCompletedIndex =
            body.IndexOf(
                "event: ToolExecutionCompleted",
                StringComparison.Ordinal);

        var messageCompletedIndex =
            body.IndexOf(
                "event: MessageCompleted",
                StringComparison.Ordinal);

        Assert.True(toolProposedIndex >= 0);

        Assert.True(
            executionStartedIndex > toolProposedIndex);

        Assert.True(
            executionCompletedIndex > executionStartedIndex);

        Assert.True(
            messageCompletedIndex > executionCompletedIndex);
    }

    [Fact]
    public async Task AgentStream_Should_Return_Unauthorized_Without_Authentication()
    {
        using var client = _factory.CreateClient();

        var request = CreateRequest("success-model");

        using var response = await PostAgentStreamAsync(
            client,
            request);

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    [Fact]
    public async Task AgentStream_Should_Return_Sse_Headers()
    {
        using var client = CreateAuthenticatedClient();

        var request = CreateRequest("success-model");

        using var response = await PostAgentStreamAsync(
            client,
            request);

        Assert.Equal(
            "text/event-stream",
            response.Content.Headers.ContentType?.MediaType);

        Assert.Contains(
            "no-cache",
            response.Headers.CacheControl?.ToString() ?? string.Empty);

        Assert.Contains(
            "no-store",
            response.Headers.CacheControl?.ToString() ?? string.Empty);

        Assert.Equal(
            "no",
            response.Headers.GetValues("X-Accel-Buffering")
                .Single());

        Assert.Equal(
            "nosniff",
            response.Headers.GetValues("X-Content-Type-Options")
                .Single());
    }

    [Fact]
    public async Task AgentStream_Should_Write_Valid_Sse_Data()
    {
        using var client = CreateAuthenticatedClient();

        var request = CreateRequest("success-model");

        using var response = await PostAgentStreamAsync(
            client,
            request);

        var body = await response.Content.ReadAsStringAsync();

        Assert.Contains(
            "event: ToolProposed\n",
            body);

        Assert.Contains(
            "data: ",
            body);

        Assert.Contains(
            "\n\n",
            body);

        var lines = body
            .Split(
                '\n',
                StringSplitOptions.RemoveEmptyEntries);

        var dataLines = lines
            .Where(line => line.StartsWith("data: "))
            .ToList();

        Assert.NotEmpty(dataLines);

        foreach (var dataLine in dataLines)
        {
            var json = dataLine["data: ".Length..];

            using var document =
                JsonDocument.Parse(json);

            Assert.True(
                document.RootElement.TryGetProperty(
                    "type",
                    out _));
        }
    }

    [Fact]
    public async Task AgentStream_Should_Propagate_Client_Cancellation()
    {
        using var client = CreateAuthenticatedClient();

        using var cancellationTokenSource =
            new CancellationTokenSource();

        var request = CreateRequest(
            "cancellation-model");

        using var httpRequest = new HttpRequestMessage(
            HttpMethod.Post,
            "/api/ai/agent/stream");

        httpRequest.Content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json");

        var responseTask = client.SendAsync(
            httpRequest,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationTokenSource.Token);

        var state =
            _factory.Services
                .GetRequiredService<AgentStreamTestState>();

        await state.StreamStarted.Task.WaitAsync(
            TimeSpan.FromSeconds(5));

        cancellationTokenSource.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            async () => await responseTask);

        await state.CancellationObserved.Task.WaitAsync(
            TimeSpan.FromSeconds(5));
    }

    private HttpClient CreateAuthenticatedClient()
    {
        var client = _factory.CreateClient();

        client.DefaultRequestHeaders.Add(
            "X-Test-Auth",
            "true");

        return client;
    }

    private static AiAgentRequest CreateRequest(
        string model)
    {
        return new AiAgentRequest
        {
            Model = model,
            Messages =
            [
                new AiMessage(
                    AiMessageRole.User,
                    "Find student")
            ],
            MaxSteps = 5
        };
    }

    private static Task<HttpResponseMessage> PostAgentStreamAsync(
        HttpClient client,
        AiAgentRequest request)
    {
        var json =
            JsonSerializer.Serialize(request);

        using var content = new StringContent(
            json,
            Encoding.UTF8,
            "application/json");

        return client.PostAsync(
            "/api/ai/agent/stream",
            content);
    }
}

public sealed class AgentStreamWebApplicationFactory :
    CustomWebApplicationFactory
{
    protected override void ConfigureWebHost(
        IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IAiAgentStreamService>();

            services.AddSingleton<IAiAgentStreamService,
                TestAiAgentStreamService>();

            services.AddAuthentication(
                    TestAuthenticationDefaults.AuthenticationScheme)
                .AddScheme<
                    AuthenticationSchemeOptions,
                    TestAuthenticationHandler>(
                    TestAuthenticationDefaults.AuthenticationScheme,
                    _ =>
                    {
                    });
        });

        base.ConfigureWebHost(builder);
    }
}

internal static class TestAuthenticationDefaults
{
    public const string AuthenticationScheme = "Test";
}

internal sealed class TestAuthenticationHandler :
    AuthenticationHandler<AuthenticationSchemeOptions>
{
    public TestAuthenticationHandler(
        Microsoft.Extensions.Options.IOptionsMonitor<
            AuthenticationSchemeOptions> options,
        Microsoft.Extensions.Logging.ILoggerFactory logger,
        System.Text.Encodings.Web.UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult>
        HandleAuthenticateAsync()
    {
        if (!Request.Headers.ContainsKey("X-Test-Auth"))
        {
            return Task.FromResult(
                AuthenticateResult.NoResult());
        }

        var claims = new[]
        {
            new Claim(
                ClaimTypes.NameIdentifier,
                "11111111-1111-1111-1111-111111111111"),

            new Claim(
                ClaimTypes.Name,
                "integration-test-user")
        };

        var identity = new ClaimsIdentity(
            claims,
            TestAuthenticationDefaults.AuthenticationScheme);

        var principal = new ClaimsPrincipal(identity);

        var ticket = new AuthenticationTicket(
            principal,
            TestAuthenticationDefaults.AuthenticationScheme);

        return Task.FromResult(
            AuthenticateResult.Success(ticket));
    }
}

internal sealed class AgentStreamTestState
{
    public TaskCompletionSource<bool> StreamStarted { get; } =
        new(TaskCreationOptions.RunContinuationsAsynchronously);

    public TaskCompletionSource<bool> CancellationObserved { get; } =
        new(TaskCreationOptions.RunContinuationsAsynchronously);
}

internal sealed class TestAiAgentStreamService :
    IAiAgentStreamService
{
    private readonly AgentStreamTestState _state;

    public TestAiAgentStreamService(
        AgentStreamTestState state)
    {
        _state = state;
    }

    public async IAsyncEnumerable<AiStreamEvent> StreamAsync(
        AiAgentRequest request,
        [System.Runtime.CompilerServices.EnumeratorCancellation]
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        cancellationToken.ThrowIfCancellationRequested();

        yield return new AiStreamEvent
        {
            Type = AiStreamEventType.ToolProposed,
            Content = "Student lookup proposed."
        };

        await Task.Yield();

        cancellationToken.ThrowIfCancellationRequested();

        if (request.Model == "cancellation-model")
        {
            _state.StreamStarted.TrySetResult(true);

            try
            {
                await Task.Delay(
                    Timeout.InfiniteTimeSpan,
                    cancellationToken);
            }
            catch (OperationCanceledException)
                when (cancellationToken.IsCancellationRequested)
            {
                _state.CancellationObserved.TrySetResult(true);
                throw;
            }
        }

        yield return new AiStreamEvent
        {
            Type = AiStreamEventType.ToolExecutionStarted
        };

        await Task.Yield();

        cancellationToken.ThrowIfCancellationRequested();

        if (request.Model == "confirmation-model")
        {
            yield return new AiStreamEvent
            {
                Type = AiStreamEventType.ConfirmationRequired,
                Content = "Confirmation is required."
            };

            yield break;
        }

        yield return new AiStreamEvent
        {
            Type = AiStreamEventType.ToolExecutionCompleted,
            Content = "Tool execution completed."
        };

        await Task.Yield();

        cancellationToken.ThrowIfCancellationRequested();

        yield return new AiStreamEvent
        {
            Type = AiStreamEventType.MessageCompleted,
            Content = "Student found."
        };
    }
}