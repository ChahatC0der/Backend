using MediatR;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.AI.DTOs;
using SchoolERP.Application.Features.AI.Guardrails;
using SchoolERP.Application.Features.AI.Services;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.AI.Commands.SemanticChat;

public sealed class SemanticChatAiCommandHandler
    : IRequestHandler<
        SemanticChatAiCommand,
        Result<AiSemanticResponse>>
{
    private readonly IAiGateway _aiGateway;
    private readonly IAiInputGuardrailService _inputGuardrailService;
    private readonly IAiOutputGuardrailService _outputGuardrailService;
    private readonly IAiSystemPromptPolicy _systemPromptPolicy;
    private readonly AiActionValidationService _actionValidationService;

    public SemanticChatAiCommandHandler(
        IAiGateway aiGateway,
        IAiInputGuardrailService inputGuardrailService,
        IAiOutputGuardrailService outputGuardrailService,
        IAiSystemPromptPolicy systemPromptPolicy,
        AiActionValidationService actionValidationService)
    {
        _aiGateway = aiGateway;
        _inputGuardrailService = inputGuardrailService;
        _outputGuardrailService = outputGuardrailService;
        _systemPromptPolicy = systemPromptPolicy;
        _actionValidationService = actionValidationService;
    }

    public async Task<Result<AiSemanticResponse>> Handle(
        SemanticChatAiCommand request,
        CancellationToken cancellationToken)
    {
        var inputGuardrailResult =
            await _inputGuardrailService.EvaluateAsync(
                request.Request.Messages,
                cancellationToken);

        if (!inputGuardrailResult.IsAllowed)
        {
            return Result.Failure<AiSemanticResponse>(
                Error.Validation(
                    string.Join(
                        " | ",
                        inputGuardrailResult.Reasons)));
        }

        var systemPromptResult =
            _systemPromptPolicy.Apply(
                request.Request);

        if (systemPromptResult.IsFailure)
        {
            return Result.Failure<AiSemanticResponse>(
                systemPromptResult.Error);
        }

        var aiResponse =
            await _aiGateway.ChatAsync(
                systemPromptResult.Value! with
                {
                    JsonMode = true
                },
                cancellationToken);

        var outputGuardrailResult =
            await _outputGuardrailService.EvaluateAsync(
                aiResponse.Content,
                cancellationToken);

        if (!outputGuardrailResult.IsAllowed)
        {
            return Result.Failure<AiSemanticResponse>(
                Error.Validation(
                    string.Join(
                        " | ",
                        outputGuardrailResult.Reasons)));
        }

        var semanticResponse =
            AiSemanticResponseParser.Parse(
                aiResponse.Content);

        if (semanticResponse.ProposedAction is null)
        {
            return Result.Success(
                semanticResponse);
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

        return Result.Success(
            semanticResponse);
    }
}