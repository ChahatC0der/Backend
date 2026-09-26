using FluentAssertions;
using Moq;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.AI.DTOs;
using SchoolERP.Application.Features.AI.Services;

namespace SchoolERP.UnitTests.Features.AI;

public sealed class AiGatewayTests
{
    [Fact]
    public async Task StreamChatAsync_should_forward_chunks_from_provider()
    {
        var provider = new Mock<IAiProvider>();

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

        var chunks = new[]
        {
            new AiStreamChunk
            {
                Content = "Hello"
            },
            new AiStreamChunk
            {
                Content = " world"
            },
            new AiStreamChunk
            {
                IsCompleted = true
            }
        };

        provider
            .Setup(x => x.StreamChatAsync(
                request,
                It.IsAny<CancellationToken>()))
            .Returns(ToAsyncEnumerable(chunks));

        var gateway = new AiGateway(provider.Object);

        var result = new List<AiStreamChunk>();

        await foreach (var chunk in gateway.StreamChatAsync(request))
        {
            result.Add(chunk);
        }

        result.Should().HaveCount(3);

        result[0].Content.Should().Be("Hello");
        result[1].Content.Should().Be(" world");
        result[2].IsCompleted.Should().BeTrue();

        provider.Verify(
            x => x.StreamChatAsync(
                request,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task StreamChatAsync_should_forward_cancellation()
    {
        var provider = new Mock<IAiProvider>();

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

        using var cancellationTokenSource =
            new CancellationTokenSource();

        provider
            .Setup(x => x.StreamChatAsync(
                request,
                It.IsAny<CancellationToken>()))
            .Returns(
                (AiChatRequest _, CancellationToken ct) =>
                    CancelledStream(ct));

        var gateway = new AiGateway(provider.Object);

        await FluentActions
            .Invoking(async () =>
            {
                await foreach (var _ in gateway.StreamChatAsync(
                    request,
                    cancellationTokenSource.Token))
                {
                }
            })
            .Should()
            .ThrowAsync<OperationCanceledException>();
    }

    private static async IAsyncEnumerable<AiStreamChunk>
        ToAsyncEnumerable(
            IEnumerable<AiStreamChunk> chunks)
    {
        foreach (var chunk in chunks)
        {
            yield return chunk;
            await Task.Yield();
        }
    }

    private static async IAsyncEnumerable<AiStreamChunk>
        CancelledStream(
            [System.Runtime.CompilerServices.EnumeratorCancellation]
            CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        await Task.Yield();

        cancellationToken.ThrowIfCancellationRequested();

        yield break;
    }
}