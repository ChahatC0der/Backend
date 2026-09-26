namespace SchoolERP.Application.Features.AI.Agent;

public enum AiAgentEventType
{
    ToolProposed = 1,
    ToolExecutionStarted = 2,
    ToolExecutionCompleted = 3,
    ConfirmationRequired = 4,
    MessageCompleted = 5,
    Error = 6
}

public sealed record AiAgentEvent
{
    public required AiAgentEventType Type { get; init; }

    public object? Data { get; init; }

    public string? Message { get; init; }

    public DateTimeOffset TimestampUtc { get; init; } =
        DateTimeOffset.UtcNow;
}