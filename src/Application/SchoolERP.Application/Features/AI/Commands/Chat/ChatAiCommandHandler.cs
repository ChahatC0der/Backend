using MediatR;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.AI.DTOs;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.AI.Commands.Chat;

public sealed class ChatAiCommandHandler
    : IRequestHandler<ChatAiCommand, Result<AiChatResponse>>
{
    private readonly IAiGateway _aiGateway;

    public ChatAiCommandHandler(IAiGateway aiGateway)
    {
        _aiGateway = aiGateway;
    }

    public async Task<Result<AiChatResponse>> Handle(
        ChatAiCommand request,
        CancellationToken cancellationToken)
    {
        var response = await _aiGateway.ChatAsync(
            request.Request,
            cancellationToken);

        return Result.Success(response);
    }
}