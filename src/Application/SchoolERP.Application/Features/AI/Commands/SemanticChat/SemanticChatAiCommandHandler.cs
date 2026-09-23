using MediatR;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.AI.DTOs;
using SchoolERP.Application.Features.AI.Services;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.AI.Commands.SemanticChat;

public sealed class SemanticChatAiCommandHandler
    : IRequestHandler<
        SemanticChatAiCommand,
        Result<AiSemanticResponse>>
{
    private readonly IAiGateway _aiGateway;
    private readonly AiActionValidationService _actionValidationService;

    public SemanticChatAiCommandHandler(
        IAiGateway aiGateway,
        AiActionValidationService actionValidationService)
    {
        _aiGateway = aiGateway;
        _actionValidationService = actionValidationService;
    }

    public async Task<Result<AiSemanticResponse>> Handle(
        SemanticChatAiCommand request,
        CancellationToken cancellationToken)
    {
        var aiResponse = await _aiGateway.ChatAsync(
            request.Request with
            {
                JsonMode = true
            },
            cancellationToken);

        var semanticResponse =
            AiSemanticResponseParser.Parse(
                aiResponse.Content);

        if (semanticResponse.ProposedAction is null)
        {
            return Result.Success(semanticResponse);
        }

        var validationResult =
            _actionValidationService.Validate(
                semanticResponse.ProposedAction);

        if (!validationResult.IsValid)
        {
            return Result.Failure<AiSemanticResponse>(
                Error.Validation(
                    string.Join(
                        " | ",
                        validationResult.Errors)));
        }

        return Result.Success(semanticResponse);
    }
}