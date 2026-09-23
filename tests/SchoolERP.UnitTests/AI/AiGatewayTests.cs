using FluentAssertions;
using Microsoft.Extensions.Options;
using SchoolERP.Application.AI;
using SchoolERP.Application.AI.Configuration;
using SchoolERP.Application.AI.Models;
using SchoolERP.Application.Common.Interfaces;

namespace SchoolERP.Application.Tests.AI;

public sealed class AiGatewayTests
{
    [Fact]
    public async Task ChatAsync_Should_Use_Configured_Provider()
    {
        var provider = new FakeAiProvider();

        var options = Options.Create(
            new AiOptions
            {
                Enabled = true,
                DefaultProvider = "FakeProvider"
            });

        var gateway = new AiGateway(
            [provider],
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

        var result = await gateway.ChatAsync(request);

        result.Content.Should().Be("Fake response");
        provider.WasCalled.Should().BeTrue();
    }

    [Fact]
    public async Task ChatAsync_Should_Reject_When_AI_Is_Disabled()
    {
        var provider = new FakeAiProvider();

        var gateway = new AiGateway(
            [provider],
            Options.Create(
                new AiOptions
                {
                    Enabled = false,
                    DefaultProvider = "FakeProvider"
                }));

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

        var act = () => gateway.ChatAsync(request);

        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("AI is currently disabled.");

        provider.WasCalled.Should().BeFalse();
    }

    private sealed class FakeAiProvider : IAiProvider
    {
        public string Name => "FakeProvider";

        public bool WasCalled { get; private set; }

        public Task<AiChatResponse> ChatAsync(
            AiChatRequest request,
            CancellationToken cancellationToken = default)
        {
            WasCalled = true;

            return Task.FromResult(
                new AiChatResponse
                {
                    Content = "Fake response",
                    Model = request.Model,
                    Provider = Name
                });
        }

        public async IAsyncEnumerable<AiStreamChunk> StreamChatAsync(
            AiChatRequest request,
            [System.Runtime.CompilerServices.EnumeratorCancellation]
            CancellationToken cancellationToken = default)
        {
            yield return new AiStreamChunk
            {
                ContentDelta = "Fake response",
                FinishReason = "stop"
            };

            await Task.CompletedTask;
        }
    }
}