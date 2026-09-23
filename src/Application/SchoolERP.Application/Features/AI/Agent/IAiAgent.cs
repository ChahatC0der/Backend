using SchoolERP.Application.Features.AI.DTOs;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.AI.Agent;

public interface IAiAgent
{
    Task<Result<AiAgentResponse>> RunAsync(
        AiAgentRequest request,
        CancellationToken cancellationToken = default);
}