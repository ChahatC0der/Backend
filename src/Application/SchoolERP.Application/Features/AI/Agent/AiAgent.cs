using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Channels;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.AI.DTOs;
using SchoolERP.Application.Features.AI.Services;
using SchoolERP.Application.Features.AI.Tools;
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

    public Task<Result<AiAgentResponse>> RunAsync(
        AiAgentRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        return RunInternalAsync(
            request,
            publishEvent: null,
            cancellationToken);
    }

    public async IAsyncEnumerable<AiAgentEvent> RunStreamAsync(
        AiAgentRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        cancellationToken.ThrowIfCancellationRequested();

        var channel = Channel.CreateUnbounded<AiAgentEvent>(
            new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = true
            });

        async ValueTask PublishAsync(
            AiAgentEvent streamEvent,
            CancellationToken token)
        {
            await channel.Writer.WriteAsync(
                streamEvent,
                token);
        }

        async Task ExecuteAsync()
        {
            try
            {
                await RunInternalAsync(
                    request,
                    PublishAsync,
                    cancellationToken);
            }
            catch (OperationCanceledException)
                when (cancellationToken.IsCancellationRequested)
            {
                // Client/request cancellation.
            }
            catch (Exception)
            {
                if (!cancellationToken.IsCancellationRequested)
                {
                    await channel.Writer.WriteAsync(
                        new AiAgentEvent
                        {
                            Type = AiAgentEventType.Error,
                            Message = "AI agent execution failed."
                        });
                }
            }
            finally
            {
                channel.Writer.TryComplete();
            }
        }

        var executionTask = ExecuteAsync();

        try
        {
            await foreach (var streamEvent in
                channel.Reader.ReadAllAsync(cancellationToken))
            {
                yield return streamEvent;
            }
        }
        finally
        {
            if (!executionTask.IsCompleted)
            {
                try
                {
                    await executionTask;
                }
                catch
                {
                    // Exception has already been translated
                    // into an Error stream event where applicable.
                }
            }
        }
    }

    private async Task<Result<AiAgentResponse>> RunInternalAsync(
        AiAgentRequest request,
        Func<AiAgentEvent, CancellationToken, ValueTask>? publishEvent,
        CancellationToken cancellationToken)
    {
        var messages = request.Messages.ToList();

        var steps = new List<AiAgentStep>();

        for (var planningStep = 1;
             planningStep <= request.MaxSteps;
             planningStep++)
        {
            cancellationToken.ThrowIfCancellationRequested();

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

            /*
             * AI returned a normal conversational response.
             */
            if (semanticResponse.ProposedAction is null)
            {
                var messageStep = new AiAgentStep
                {
                    StepNumber = steps.Count + 1,
                    Kind = AiAgentStepKind.Message,
                    Message = semanticResponse.Message
                };

                steps.Add(messageStep);

                await PublishEventAsync(
                    publishEvent,
                    new AiAgentEvent
                    {
                        Type = AiAgentEventType.MessageCompleted,
                        Message = semanticResponse.Message,
                        Data = new
                        {
                            State = AiAgentState.Completed
                        }
                    },
                    cancellationToken);

                return Result.Success(
                    new AiAgentResponse
                    {
                        State = AiAgentState.Completed,
                        Message = semanticResponse.Message,
                        Steps = steps
                    });
            }

            /*
             * AI proposed an action.
             */
            var action = semanticResponse.ProposedAction;

            var validationResult =
                _actionValidationService.Validate(action);

            if (!validationResult.IsValid)
            {
                var errorMessage = string.Join(
                    " | ",
                    validationResult.Errors);

                await PublishEventAsync(
                    publishEvent,   
                    new AiAgentEvent
                    {
                        Type = AiAgentEventType.Error,
                        Message = errorMessage
                    },
                    cancellationToken);

                return Result.Failure<AiAgentResponse>(
                    Error.Validation(errorMessage));
            }

            /*
             * Tell the client which tool/action was proposed.
             */
            await PublishEventAsync(
                publishEvent,
                new AiAgentEvent
                {
                    Type = AiAgentEventType.ToolProposed,
                    Data = action,
                    Message = semanticResponse.Message
                },
                cancellationToken);

            var actionStep = new AiAgentStep
            {
                StepNumber = steps.Count + 1,
                Kind = AiAgentStepKind.ActionProposal,
                Message = semanticResponse.Message,
                Action = action
            };

            steps.Add(actionStep);

            /*
             * Tool execution started.
             */
            await PublishEventAsync(
                publishEvent,
                new AiAgentEvent
                {
                    Type = AiAgentEventType.ToolExecutionStarted,
                    Data = new
                    {
                        Tool = validationResult.Tool!.Name,
                        Version = validationResult.Tool.Version
                    }
                },
                cancellationToken);

            var executionResult =
                await _toolExecutor.ExecuteAsync(
                    validationResult.Tool,
                    action,
                    cancellationToken);

            if (executionResult.IsFailure)
            {
                await PublishEventAsync(
                    publishEvent,
                    new AiAgentEvent
                    {
                        Type = AiAgentEventType.Error,
                        Data = executionResult.Error
                    },
                    cancellationToken);

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

            /*
             * IMPORTANT:
             *
             * ConfirmationRequired is a terminal state for the
             * current agent execution.
             *
             * Do not continue planning before the user confirms.
             */
            if (toolResult.RequiresConfirmation)
            {
                await PublishEventAsync(
                    publishEvent,
                    new AiAgentEvent
                    {
                        Type = AiAgentEventType.ConfirmationRequired,
                        Data = toolResult,
                        Message = semanticResponse.Message
                    },
                    cancellationToken);

                return Result.Success(
                    new AiAgentResponse
                    {
                        State = AiAgentState.WaitingForTool,
                        Message = semanticResponse.Message,
                        Steps = steps
                    });
            }

            /*
             * Tool executed successfully.
             */
            await PublishEventAsync(
                publishEvent,
                new AiAgentEvent
                {
                    Type = AiAgentEventType.ToolExecutionCompleted,
                    Data = new
                    {
                        Tool = toolResult.ToolName,
                        Message = toolResult.Message
                    }
                },
                cancellationToken);

            /*
             * Add the tool result to the AI conversation.
             */
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

        var maxStepError =
            $"AI agent reached the maximum step limit of {request.MaxSteps} without completing the task.";

        await PublishEventAsync(
            publishEvent,
            new AiAgentEvent
            {
                Type = AiAgentEventType.Error,
                Message = maxStepError
            },
            cancellationToken);

        return Result.Failure<AiAgentResponse>(
            Error.Validation(maxStepError));
    }

    private static ValueTask PublishEventAsync(
    Func<AiAgentEvent, CancellationToken, ValueTask>? publishEvent,
    AiAgentEvent streamEvent,
    CancellationToken cancellationToken)
{
    if (publishEvent is null)
    {
        return ValueTask.CompletedTask;
    }

    return publishEvent(
        streamEvent,
        cancellationToken);
}
}