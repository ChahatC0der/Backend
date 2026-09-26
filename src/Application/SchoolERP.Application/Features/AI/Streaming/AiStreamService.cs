using System.Runtime.CompilerServices;
using SchoolERP.Application.Common.Interfaces;
using System.Text.Json;
using SchoolERP.Application.Features.AI.Streaming.DTOs;
using SchoolERP.Application.Features.AI.DTOs;

namespace SchoolERP.Application.Features.AI.Streaming;

public sealed class AiStreamService : IAiStreamService
{
    private readonly IAiGateway _aiGateway;

    public AiStreamService(IAiGateway aiGateway)
    {
        _aiGateway = aiGateway;
    }

    public async IAsyncEnumerable<AiStreamEvent> StreamAsync(
        AiChatRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        cancellationToken.ThrowIfCancellationRequested();

        yield return new AiStreamEvent
        {
            Type = AiStreamEventType.MessageStarted
        };

        var result = await ReadGatewayStreamAsync(
            request,
            cancellationToken);

        if (result.ErrorMessage is not null)
        {
            yield return new AiStreamEvent
            {
                Type = AiStreamEventType.Error,
                Content = result.ErrorMessage
            };

            yield break;
        }

        foreach (var chunk in result.Chunks)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!string.IsNullOrEmpty(chunk.Content))
            {
                yield return new AiStreamEvent
                {
                    Type = AiStreamEventType.MessageDelta,
                    Content = chunk.Content
                };
            }

            if (chunk.IsCompleted)
            {
                var data = new AiStreamMessageCompletedData
                {
                    FinishReason = chunk.FinishReason,
                    Model = chunk.Model,
                    Provider = chunk.Provider
                };

                yield return new AiStreamEvent
                {
                    Type = AiStreamEventType.MessageCompleted,
                    Data = JsonSerializer.SerializeToElement(data)
                };
            }
        }
    }

    private async Task<GatewayStreamResult> ReadGatewayStreamAsync(
        AiChatRequest request,
        CancellationToken cancellationToken)
    {
        var chunks = new List<AiStreamChunk>();

        try
        {
            await foreach (var chunk in _aiGateway.StreamChatAsync(
                request,
                cancellationToken))
            {
                chunks.Add(chunk);
            }

            return new GatewayStreamResult(chunks, null);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return new GatewayStreamResult([], ex.Message);
        }
    }

    private sealed record GatewayStreamResult(
        IReadOnlyList<AiStreamChunk> Chunks,
        string? ErrorMessage);
}