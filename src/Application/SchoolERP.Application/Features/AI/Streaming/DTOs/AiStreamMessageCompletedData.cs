using SchoolERP.Application.Features.AI.DTOs;

namespace SchoolERP.Application.Features.AI.Streaming.DTOs;

public sealed record AiStreamMessageCompletedData
{
    public AiAgentState? State { get; init; }

    public string? FinishReason { get; init; }

    public string? Model { get; init; }

    public string? Provider { get; init; }
}