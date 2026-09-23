using System.Net;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Options;
using SchoolERP.Application.AI.Configuration;
using SchoolERP.Application.AI.Models;
using SchoolERP.Infrastructure.AI.Providers;

namespace SchoolERP.Infrastructure.Tests.AI;

public sealed class OpenRouterAiProviderTests
{
    [Fact]
    public async Task ChatAsync_Should_Map_Provider_Response()
    {
        var handler = new FakeHttpMessageHandler(
            """
            {
              "choices": [
                {
                  "message": {
                    "role": "assistant",
                    "content": "Hello from AI"
                  },
                  "finish_reason": "stop"
                }
              ],
              "usage": {
                "prompt_tokens": 10,
                "completion_tokens": 5,
                "total_tokens": 15
              }
            }
            """);

        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri(
                "https://openrouter.ai/api/v1/")
        };

        var options = Options.Create(
            new AiOptions
            {
                Enabled = true,
                DefaultProvider = "OpenRouter",
                DefaultModel = "test-model",
                BaseUrl = "https://openrouter.ai/api/v1/",
                ApiKey = "test-key"
            });

        var provider = new OpenRouterAiProvider(
            httpClient,
            options);

        var request = new AiChatRequest
        {
            Model = "test-model",
            Messages =
            [
                new AiMessage(
                    AiMessageRole.User,
                    "Hello")
            ]
        };

        var result = await provider.ChatAsync(request);

        result.Content.Should().Be("Hello from AI");
        result.Provider.Should().Be("OpenRouter");
        result.Model.Should().Be("test-model");

        result.Usage.Should().NotBeNull();
        result.Usage!.PromptTokens.Should().Be(10);
        result.Usage.CompletionTokens.Should().Be(5);
        result.Usage.TotalTokens.Should().Be(15);
    }

    [Fact]
    public async Task ChatAsync_Should_Send_Authorization_Header()
    {
        var handler = new FakeHttpMessageHandler(
            """
            {
              "choices": [
                {
                  "message": {
                    "content": "OK"
                  },
                  "finish_reason": "stop"
                }
              ]
            }
            """);

        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri(
                "https://openrouter.ai/api/v1/")
        };

        var provider = new OpenRouterAiProvider(
            httpClient,
            Options.Create(
                new AiOptions
                {
                    Enabled = true,
                    DefaultProvider = "OpenRouter",
                    ApiKey = "test-key"
                }));

        var request = new AiChatRequest
        {
            Model = "test-model",
            Messages =
            [
                new AiMessage(
                    AiMessageRole.User,
                    "Test")
            ]
        };

        await provider.ChatAsync(request);

        handler.LastRequest.Should().NotBeNull();
        handler.LastRequest!.RequestUri!.AbsolutePath
            .Should().Be("/api/v1/chat/completions");

        handler.LastRequest.Headers.Authorization
            .Should().NotBeNull();

        handler.LastRequest.Headers.Authorization!
            .Scheme.Should().Be("Bearer");

        handler.LastRequest.Headers.Authorization!
            .Parameter.Should().Be("test-key");
    }

    private sealed class FakeHttpMessageHandler : HttpMessageHandler
    {
        private readonly string _response;

        public HttpRequestMessage? LastRequest { get; private set; }

        public FakeHttpMessageHandler(string response)
        {
            _response = response;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            LastRequest = request;

            var response = new HttpResponseMessage(
                HttpStatusCode.OK)
            {
                Content = new StringContent(
                    _response,
                    Encoding.UTF8,
                    "application/json")
            };

            return Task.FromResult(response);
        }
    }
}