using SchoolERP.Application.Features.AI.DTOs;

namespace SchoolERP.Application.Features.AI.Streaming;

public interface IAiAgentStreamService
{
    IAsyncEnumerable<AiStreamEvent> StreamAsync(
        AiAgentRequest request,
        CancellationToken cancellationToken = default);
}