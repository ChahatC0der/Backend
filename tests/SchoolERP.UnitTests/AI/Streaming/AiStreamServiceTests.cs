using FluentAssertions;
using Moq;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.AI.DTOs;
using SchoolERP.Application.Features.AI.Streaming;

namespace SchoolERP.UnitTests.Features.AI.Streaming;

public sealed class AiStreamServiceTests
{
    [Fact]
    public async Task StreamAsync_should_emit_message_started_first()
    {
        var gateway = new Mock<IAiGateway>();

        var request = CreateRequest();

        gateway
            .Setup(x => x.StreamChatAsync(
                request,
                It.IsAny<CancellationToken>()))
            .Returns(EmptyStream());

        var service = new AiStreamService(
            gateway.Object);

        var events = await CollectEventsAsync(
            service.StreamAsync(request));

        events.Should().NotBeEmpty();

        events[0].Type
            .Should()
            .Be(AiStreamEventType.MessageStarted);
    }

    [Fact]
    public async Task StreamAsync_should_convert_content_chunks_to_message_delta_events()
    {
        var gateway = new Mock<IAiGateway>();

        var request = CreateRequest();

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

        gateway
            .Setup(x => x.StreamChatAsync(
                request,
                It.IsAny<CancellationToken>()))
            .Returns(ToAsyncEnumerable(chunks));

        var service = new AiStreamService(
            gateway.Object);

        var events = await CollectEventsAsync(
            service.StreamAsync(request));

        events
            .Where(x => x.Type == AiStreamEventType.MessageDelta)
            .Select(x => x.Content)
            .Should()
            .Equal(
                "Hello",
                " world");
    }

    [Fact]
    public async Task StreamAsync_should_emit_message_completed_when_gateway_completes()
    {
        var gateway = new Mock<IAiGateway>();

        var request = CreateRequest();

        var completionChunk = new AiStreamChunk
        {
            IsCompleted = true
        };

        gateway
            .Setup(x => x.StreamChatAsync(
                request,
                It.IsAny<CancellationToken>()))
            .Returns(
                ToAsyncEnumerable(
                    [completionChunk]));

        var service = new AiStreamService(
            gateway.Object);

        var events = await CollectEventsAsync(
            service.StreamAsync(request));

        events
            .Should()
            .ContainSingle(
                x => x.Type == AiStreamEventType.MessageCompleted);
    }

    [Fact]
    public async Task StreamAsync_should_ignore_empty_content_chunks()
    {
        var gateway = new Mock<IAiGateway>();

        var request = CreateRequest();

        var chunks = new[]
        {
            new AiStreamChunk
            {
                Content = null
            },
            new AiStreamChunk
            {
                Content = string.Empty
            },
            new AiStreamChunk
            {
                Content = "Hello"
            },
            new AiStreamChunk
            {
                IsCompleted = true
            }
        };

        gateway
            .Setup(x => x.StreamChatAsync(
                request,
                It.IsAny<CancellationToken>()))
            .Returns(ToAsyncEnumerable(chunks));

        var service = new AiStreamService(
            gateway.Object);

        var events = await CollectEventsAsync(
            service.StreamAsync(request));

        events
            .Where(x => x.Type == AiStreamEventType.MessageDelta)
            .Select(x => x.Content)
            .Should()
            .Equal("Hello");
    }

    [Fact]
    public async Task StreamAsync_should_emit_error_when_gateway_fails()
    {
        var gateway = new Mock<IAiGateway>();

        var request = CreateRequest();

        gateway
            .Setup(x => x.StreamChatAsync(
                request,
                It.IsAny<CancellationToken>()))
            .Returns(FailingStream());

        var service = new AiStreamService(
            gateway.Object);

        var events = await CollectEventsAsync(
            service.StreamAsync(request));

        var errorEvent = events
            .Should()
            .ContainSingle(
                x => x.Type == AiStreamEventType.Error)
            .Which;

        errorEvent.Content
            .Should()
            .Be("Gateway failure.");
    }

    [Fact]
    public async Task StreamAsync_should_propagate_cancellation()
    {
        var gateway = new Mock<IAiGateway>();

        var request = CreateRequest();

        using var cancellationTokenSource =
            new CancellationTokenSource();

        gateway
            .Setup(x => x.StreamChatAsync(
                request,
                It.IsAny<CancellationToken>()))
            .Returns(
                CancelledStream(
                    cancellationTokenSource.Token));

        var service = new AiStreamService(
            gateway.Object);

        var action = async () =>
        {
            await foreach (var _ in service.StreamAsync(
                request,
                cancellationTokenSource.Token))
            {
            }
        };

        await action
            .Should()
            .ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task StreamAsync_should_reject_null_request()
    {
        var gateway = new Mock<IAiGateway>();

        var service = new AiStreamService(
            gateway.Object);

        Func<Task> action = async () =>
        {
            await foreach (var _ in service.StreamAsync(
                null!))
            {
            }
        };

        await action
            .Should()
            .ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task StreamAsync_should_forward_request_to_gateway()
    {
        var gateway = new Mock<IAiGateway>();

        var request = CreateRequest();

        gateway
            .Setup(x => x.StreamChatAsync(
                request,
                It.IsAny<CancellationToken>()))
            .Returns(EmptyStream());

        var service = new AiStreamService(
            gateway.Object);

        await foreach (var _ in service.StreamAsync(request))
        {
        }

        gateway.Verify(
            x => x.StreamChatAsync(
                request,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private static AiChatRequest CreateRequest()
    {
        return new AiChatRequest
        {
            Model = "test-model",
            Messages =
            [
                new AiMessage(
                    AiMessageRole.User,
                    "Hello")
            ]
        };
    }

    private static async Task<List<AiStreamEvent>>
        CollectEventsAsync(
            IAsyncEnumerable<AiStreamEvent> stream)
    {
        var events = new List<AiStreamEvent>();

        await foreach (var streamEvent in stream)
        {
            events.Add(streamEvent);
        }

        return events;
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
        EmptyStream()
    {
        await Task.CompletedTask;

        yield break;
    }

    private static async IAsyncEnumerable<AiStreamChunk>
        FailingStream()
    {
        await Task.Yield();

        throw new InvalidOperationException(
            "Gateway failure.");
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