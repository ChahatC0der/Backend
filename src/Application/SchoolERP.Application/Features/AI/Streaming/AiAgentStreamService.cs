using System.Runtime.CompilerServices;
using System.Text.Json;
using SchoolERP.Application.Features.AI.Agent;
using SchoolERP.Application.Features.AI.DTOs;
using SchoolERP.Application.Features.AI.Streaming.DTOs;

namespace SchoolERP.Application.Features.AI.Streaming;

public sealed class AiAgentStreamService : IAiAgentStreamService
{
    private readonly IAiAgent _aiAgent;

    public AiAgentStreamService(IAiAgent aiAgent)
    {
        _aiAgent = aiAgent;
    }

    public async IAsyncEnumerable<AiStreamEvent> StreamAsync(
        AiAgentRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        await foreach (var agentEvent in _aiAgent.RunStreamAsync(
            request,
            cancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            yield return Map(agentEvent);
        }
    }

    private static AiStreamEvent Map(
        AiAgentEvent agentEvent)
    {
        return agentEvent.Type switch
        {
            AiAgentEventType.ToolProposed =>
                CreateEvent(
                    AiStreamEventType.ToolProposed,
                    agentEvent,
                    CreateToolProposedData(agentEvent.Data)),

            AiAgentEventType.ToolExecutionStarted =>
                CreateEvent(
                    AiStreamEventType.ToolExecutionStarted,
                    agentEvent,
                    CreateToolExecutionStartedData(agentEvent.Data)),

            AiAgentEventType.ToolExecutionCompleted =>
                CreateEvent(
                    AiStreamEventType.ToolExecutionCompleted,
                    agentEvent,
                    CreateToolExecutionCompletedData(agentEvent.Data)),

            AiAgentEventType.ConfirmationRequired =>
                CreateEvent(
                    AiStreamEventType.ConfirmationRequired,
                    agentEvent,
                    CreateConfirmationRequiredData(agentEvent.Data)),

            AiAgentEventType.MessageCompleted =>
                CreateEvent(
                    AiStreamEventType.MessageCompleted,
                    agentEvent,
                    CreateMessageCompletedData(agentEvent)),

            AiAgentEventType.Error =>
                CreateEvent(
                    AiStreamEventType.Error,
                    agentEvent,
                    CreateErrorData(agentEvent)),

            _ =>
                throw new ArgumentOutOfRangeException(
                    nameof(agentEvent.Type),
                    agentEvent.Type,
                    "Unsupported AI agent event type.")
        };
    }

    private static AiStreamEvent CreateEvent(
        AiStreamEventType type,
        AiAgentEvent agentEvent,
        object? data)
    {
        return new AiStreamEvent
        {
            Type = type,
            Content = agentEvent.Message,
            Data = SerializeData(data),
            TimestampUtc = agentEvent.TimestampUtc
        };
    }

    private static AiStreamToolProposedData?
        CreateToolProposedData(object? data)
    {
        if (data is null)
        {
            return null;
        }

        return data switch
        {
            AiActionProposal action =>
                new AiStreamToolProposedData
                {
                    ActionName = action.ActionName,
                    Version = action.Version,
                    Arguments = action.Arguments
                },

            _ => DeserializeData<AiStreamToolProposedData>(data)
        };
    }

    private static AiStreamToolExecutionStartedData?
    CreateToolExecutionStartedData(object? data)
    {
        if (data is null)
        {
            return null;
        }

        var element = JsonSerializer.SerializeToElement(data);

        var toolName =
            element.TryGetProperty("Tool", out var toolProperty)
                ? toolProperty.GetString()
                : element.TryGetProperty("ToolName", out var toolNameProperty)
                    ? toolNameProperty.GetString()
                    : null;

        var version =
            element.TryGetProperty("Version", out var versionProperty)
                ? versionProperty.GetInt32()
                : 1;

        if (string.IsNullOrWhiteSpace(toolName))
        {
            throw new InvalidOperationException(
                "Tool execution started event does not contain a tool name.");
        }

        return new AiStreamToolExecutionStartedData
        {
            ToolName = toolName,
            Version = version
        };
    }

    private static AiStreamToolExecutionCompletedData?
    CreateToolExecutionCompletedData(object? data)
    {
        if (data is null)
        {
            return null;
        }

        var element = JsonSerializer.SerializeToElement(data);

        var toolName =
            element.TryGetProperty("Tool", out var toolProperty)
                ? toolProperty.GetString()
                : element.TryGetProperty("ToolName", out var toolNameProperty)
                    ? toolNameProperty.GetString()
                    : null;

        var message =
            element.TryGetProperty("Message", out var messageProperty)
                ? messageProperty.GetString()
                : null;

        if (string.IsNullOrWhiteSpace(toolName))
        {
            throw new InvalidOperationException(
                "Tool execution completed event does not contain a tool name.");
        }

        return new AiStreamToolExecutionCompletedData
        {
            ToolName = toolName,
            Message = message
        };
    }

    private static AiStreamConfirmationRequiredData?
        CreateConfirmationRequiredData(object? data)
    {
        if (data is null)
        {
            return null;
        }

        return data switch
        {
            AiToolExecutionResult result
                when result.Confirmation is not null =>
                new AiStreamConfirmationRequiredData
                {
                    ToolName = result.ToolName,
                    Message = result.Message,
                    Confirmation = result.Confirmation
                },

            _ =>
                DeserializeData<AiStreamConfirmationRequiredData>(
                    data)
        };
    }

    private static AiStreamMessageCompletedData
        CreateMessageCompletedData(
            AiAgentEvent agentEvent)
    {
        var data = DeserializeData<AiStreamMessageCompletedData>(
            agentEvent.Data);

        return data ?? new AiStreamMessageCompletedData
        {
            State = AiAgentState.Completed
        };
    }

    private static AiStreamErrorData CreateErrorData(
        AiAgentEvent agentEvent)
    {
        return new AiStreamErrorData
        {
            Message = agentEvent.Message,
            Details = SerializeData(
                agentEvent.Data)
        };
    }

    private static JsonElement? SerializeData(
        object? data)
    {
        if (data is null)
        {
            return null;
        }

        return JsonSerializer.SerializeToElement(data);
    }

    private static T? DeserializeData<T>(
        object? data)
    {
        if (data is null)
        {
            return default;
        }

        if (data is T typedData)
        {
            return typedData;
        }

        var element =
            JsonSerializer.SerializeToElement(data);

        return element.Deserialize<T>();
    }
}