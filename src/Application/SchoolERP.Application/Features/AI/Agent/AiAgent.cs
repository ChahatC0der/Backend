using System.Text.Json;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.AI.DTOs;
using SchoolERP.Application.Features.AI.Services;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.AI.Agent;

public sealed class AiAgent : IAiAgent
{
    private readonly IAiGateway _aiGateway;
    private readonly AiActionValidationService _actionValidationService;
    private readonly IAiToolExecutor _toolExecutor;

    public AiAgent(
        IAiGateway aiGateway,
        AiActionValidationService actionValidationService,
        IAiToolExecutor toolExecutor)
    {
        _aiGateway = aiGateway;
        _actionValidationService = actionValidationService;
        _toolExecutor = toolExecutor;
    }

    public async Task<Result<AiAgentResponse>> RunAsync(
        AiAgentRequest request,
        CancellationToken cancellationToken = default)
    {
        var messages = request.Messages.ToList();
        var steps = new List<AiAgentStep>();

        for (var planningStep = 1;
             planningStep <= request.MaxSteps;
             planningStep++)
        {
            var aiResponse = await _aiGateway.ChatAsync(
                new AiChatRequest
                {
                    Model = request.Model,
                    Messages = messages,
                    JsonMode = true
                },
                cancellationToken);

            var semanticResponse =
                AiSemanticResponseParser.Parse(
                    aiResponse.Content);

            // AI returned a normal message.
            if (semanticResponse.ProposedAction is null)
            {
                var messageStep = new AiAgentStep
                {
                    StepNumber = steps.Count + 1,
                    Kind = AiAgentStepKind.Message,
                    Message = semanticResponse.Message
                };

                steps.Add(messageStep);

                return Result.Success(
                    new AiAgentResponse
                    {
                        State = AiAgentState.Completed,
                        Message = semanticResponse.Message,
                        Steps = steps
                    });
            }

            // AI proposed an action.
            var action = semanticResponse.ProposedAction;

            var validationResult =
                _actionValidationService.Validate(action);

            if (!validationResult.IsValid)
            {
                return Result.Failure<AiAgentResponse>(
                    Error.Validation(
                        string.Join(
                            " | ",
                            validationResult.Errors)));
            }

            var actionStep = new AiAgentStep
            {
                StepNumber = steps.Count + 1,
                Kind = AiAgentStepKind.ActionProposal,
                Message = semanticResponse.Message,
                Action = action
            };

            steps.Add(actionStep);

            // Execute validated tool.
            var executionResult =
                await _toolExecutor.ExecuteAsync(
                    validationResult.Tool!,
                    action,
                    cancellationToken);

            if (executionResult.IsFailure)
            {
                return Result.Failure<AiAgentResponse>(
                    executionResult.Error);
            }

            var toolResult = executionResult.Value!;

            var toolStep = new AiAgentStep
            {
                StepNumber = steps.Count + 1,
                Kind = AiAgentStepKind.ToolResult,
                ToolResult = toolResult
            };

            steps.Add(toolStep);

            // Add tool result back to the conversation.
            messages.Add(
                new AiMessage(
                    AiMessageRole.Tool,
                    JsonSerializer.Serialize(
                        new
                        {
                            type = "tool_result",
                            tool = toolResult.ToolName,
                            output = toolResult.Output,
                            message = toolResult.Message
                        })));
        }

        return Result.Failure<AiAgentResponse>(
            Error.Validation(
                $"AI agent reached the maximum step limit of {request.MaxSteps} without completing the task."));
    }
}