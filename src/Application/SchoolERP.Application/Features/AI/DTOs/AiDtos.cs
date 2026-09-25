using SchoolERP.Application.Features.AI.Confirmation;
using System.Text.Json;

namespace SchoolERP.Application.Features.AI.DTOs;

public sealed record AiMessage(
    AiMessageRole Role,
    string Content);

public enum AiMessageRole
{
    System,
    User,
    Assistant,
    Tool
}

public sealed record AiChatRequest
{
    public required string Model { get; init; }

    public required IReadOnlyList<AiMessage> Messages { get; init; }

    public float? Temperature { get; init; }

    public int? MaxTokens { get; init; }

    public string? SystemPrompt { get; init; }

    public bool JsonMode { get; init; }
}

public sealed record AiUsage(
    int PromptTokens,
    int CompletionTokens,
    int TotalTokens);

public sealed record AiChatResponse
{
    public required string Content { get; init; }

    public string? Model { get; init; }

    public string? Provider { get; init; }

    public AiUsage? Usage { get; init; }
}

public sealed record AiStreamChunk
{
    public string? ContentDelta { get; init; }

    public string? FinishReason { get; init; }

    public AiUsage? Usage { get; init; }

    public IReadOnlyDictionary<string, object?>? Metadata { get; init; }
}

public sealed record AiActionProposal
{
    public required string ActionName { get; init; }

    public int Version { get; init; } = 1;

    public IReadOnlyDictionary<string, JsonElement> Arguments { get; init; }
        = new Dictionary<string, JsonElement>();
}

public sealed record AiSemanticResponse
{
    public required AiResponseKind Kind { get; init; }

    public string? Message { get; init; }

    public AiActionProposal? ProposedAction { get; init; }
}

public enum AiResponseKind
{
    Message,
    Action
}

public sealed record AiAgentRequest
{
    public required string Model { get; init; }

    public required IReadOnlyList<AiMessage> Messages { get; init; }

    public int MaxSteps { get; init; } = 5;
}

public sealed record AiAgentStep
{
    public int StepNumber { get; init; }

    public AiAgentStepKind Kind { get; init; }

    public string? Message { get; init; }

    public AiActionProposal? Action { get; init; }

    public AiToolExecutionResult? ToolResult { get; init; }
}

public enum AiAgentStepKind
{
    Message,
    ActionProposal,
    ToolResult
}

public sealed record AiAgentResponse
{
    public AiAgentState State { get; init; }

    public string? Message { get; init; }

    public IReadOnlyList<AiAgentStep> Steps { get; init; } = [];

    public AiActionProposal? PendingAction { get; init; }
}

public enum AiAgentState
{
    Planning,
    WaitingForTool,
    Completed
}

public sealed record AiToolExecutionResult
{
    public required string ToolName { get; init; }

    public object? Output { get; init; }

    public string? Message { get; init; }

    public AiToolExecutionStatus Status { get; init; }
        = AiToolExecutionStatus.Executed;

    public AiConfirmationRequirement? Confirmation { get; init; }

    public bool RequiresConfirmation
        => Status == AiToolExecutionStatus.ConfirmationRequired;

    public static AiToolExecutionResult Executed(
        string toolName,
        object? output = null,
        string? message = null)
    {
        return new AiToolExecutionResult
        {
            ToolName = toolName,
            Output = output,
            Message = message,
            Status = AiToolExecutionStatus.Executed,
            Confirmation = null
        };
    }

    public static AiToolExecutionResult ConfirmationRequired(
        AiConfirmationRequirement requirement)
    {
        return new AiToolExecutionResult
        {
            ToolName = requirement.ToolName,
            Output = null,
            Message =
                $"Confirmation is required before executing AI tool '{requirement.ToolName}'.",
            Status = AiToolExecutionStatus.ConfirmationRequired,
            Confirmation = requirement
        };
    }
}

public enum AiToolExecutionStatus
{
    Executed = 1,
    ConfirmationRequired = 2
}

public sealed record AiExecutionContext
{
    public bool IsAuthenticated { get; init; }

    public Guid? UserId { get; init; }

    public Guid? TenantId { get; init; }

    public Guid? BranchId { get; init; }

    public IReadOnlySet<string> Permissions { get; init; }
        = new HashSet<string>(
            StringComparer.OrdinalIgnoreCase);
}

public sealed record AiGuardrailResult
{
    public bool IsAllowed { get; init; }

    public IReadOnlyCollection<string> Reasons { get; init; }
        = [];

    public static AiGuardrailResult Allowed()
        => new()
        {
            IsAllowed = true
        };

    public static AiGuardrailResult Blocked(
        params string[] reasons)
        => new()
        {
            IsAllowed = false,
            Reasons = reasons
        };
}