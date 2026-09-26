namespace SchoolERP.Application.Features.AI.Streaming.DTOs;

public sealed record AiStreamToolExecutionStartedData
{
    public required string ToolName { get; init; }

    public int Version { get; init; }
}