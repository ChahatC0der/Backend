using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.AI.DTOs;

namespace SchoolERP.Application.Features.AI.Commands.RunAgent;

public sealed record RunAiAgentCommand(
    AiAgentRequest Request
) : ICommand<AiAgentResponse>;