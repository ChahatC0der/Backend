using MediatR;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.AI.DTOs;
using SchoolERP.Application.Features.AI.Guardrails;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.AI.Commands.Chat;

public sealed class ChatAiCommandHandler
    : IRequestHandler<ChatAiCommand, Result<AiChatResponse>>
{
    private readonly IAiGateway _aiGateway;
    private readonly IAiInputGuardrailService _inputGuardrailService;
    private readonly IAiOutputGuardrailService _outputGuardrailService;
    private readonly IAiSystemPromptPolicy _systemPromptPolicy;

    public ChatAiCommandHandler(
        IAiGateway aiGateway,
        IAiInputGuardrailService inputGuardrailService,
        IAiOutputGuardrailService outputGuardrailService,
        IAiSystemPromptPolicy systemPromptPolicy)
    {
        _aiGateway = aiGateway;
        _inputGuardrailService = inputGuardrailService;
        _outputGuardrailService = outputGuardrailService;
        _systemPromptPolicy = systemPromptPolicy;
    }

    public async Task<Result<AiChatResponse>> Handle(
        ChatAiCommand request,
        CancellationToken cancellationToken)
    {
        var inputGuardrailResult =
            await _inputGuardrailService.EvaluateAsync(
                request.Request.Messages,
                cancellationToken);

        if (!inputGuardrailResult.IsAllowed)
        {
            return Result.Failure<AiChatResponse>(
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
            return Result.Failure<AiChatResponse>(
                systemPromptResult.Error);
        }

        var response =
            await _aiGateway.ChatAsync(
                systemPromptResult.Value!,
                cancellationToken);

        var outputGuardrailResult =
            await _outputGuardrailService.EvaluateAsync(
                response.Content,
                cancellationToken);

        if (!outputGuardrailResult.IsAllowed)
        {
            return Result.Failure<AiChatResponse>(
                Error.Validation(
                    string.Join(
                        " | ",
                        outputGuardrailResult.Reasons)));
        }

        return Result.Success(response);
    }
}