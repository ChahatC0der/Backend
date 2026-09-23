using SchoolERP.Application.Features.AI.DTOs;

namespace SchoolERP.Application.Common.Interfaces;

public interface IAiGateway
{
    Task<AiChatResponse> ChatAsync(
        AiChatRequest request,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<AiStreamChunk> StreamChatAsync(
        AiChatRequest request,
        CancellationToken cancellationToken = default);
}