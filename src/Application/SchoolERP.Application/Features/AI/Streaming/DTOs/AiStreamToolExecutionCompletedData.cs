namespace SchoolERP.Application.Features.AI.Streaming.DTOs;

public sealed record AiStreamToolExecutionCompletedData
{
    public required string ToolName { get; init; }

    public string? Message { get; init; }
}