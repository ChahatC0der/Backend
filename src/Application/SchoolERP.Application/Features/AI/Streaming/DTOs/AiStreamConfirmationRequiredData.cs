using SchoolERP.Application.Features.AI.Confirmation;

namespace SchoolERP.Application.Features.AI.Streaming.DTOs;

public sealed record AiStreamConfirmationRequiredData
{
    public required string ToolName { get; init; }

    public string? Message { get; init; }

    public required AiConfirmationRequirement Confirmation { get; init; }
}