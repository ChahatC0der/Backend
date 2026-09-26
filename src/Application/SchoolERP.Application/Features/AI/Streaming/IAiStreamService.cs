using SchoolERP.Application.Features.AI.DTOs;

namespace SchoolERP.Application.Features.AI.Streaming;

public interface IAiStreamService
{
    IAsyncEnumerable<AiStreamEvent> StreamAsync(
        AiChatRequest request,
        CancellationToken cancellationToken = default);
}