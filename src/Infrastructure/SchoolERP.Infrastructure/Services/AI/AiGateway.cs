using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.AI.DTOs;

namespace SchoolERP.Application.Features.AI.Services;

public sealed class AiGateway : IAiGateway
{
    private readonly IAiProvider _provider;

    public AiGateway(IAiProvider provider)
    {
        _provider = provider;
    }

    public async Task<AiChatResponse> ChatAsync(
        AiChatRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        return await _provider.ChatAsync(
            request,
            cancellationToken);
    }

    public async IAsyncEnumerable<AiStreamChunk> StreamChatAsync(
        AiChatRequest request,
        [System.Runtime.CompilerServices.EnumeratorCancellation]
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        await foreach (var chunk in _provider.StreamChatAsync(
                           request,
                           cancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            yield return chunk;
        }
    }
}