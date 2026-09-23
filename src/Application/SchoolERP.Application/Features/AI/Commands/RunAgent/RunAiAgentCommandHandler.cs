using MediatR;
using SchoolERP.Application.Features.AI.Agent;
using SchoolERP.Application.Features.AI.DTOs;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.AI.Commands.RunAgent;

public sealed class RunAiAgentCommandHandler
    : IRequestHandler<
        RunAiAgentCommand,
        Result<AiAgentResponse>>
{
    private readonly IAiAgent _aiAgent;

    public RunAiAgentCommandHandler(IAiAgent aiAgent)
    {
        _aiAgent = aiAgent;
    }

    public async Task<Result<AiAgentResponse>> Handle(
        RunAiAgentCommand request,
        CancellationToken cancellationToken)
    {
        return await _aiAgent.RunAsync(
            request.Request,
            cancellationToken);
    }
}