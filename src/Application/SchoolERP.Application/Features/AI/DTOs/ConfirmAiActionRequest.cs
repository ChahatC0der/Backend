namespace SchoolERP.Application.Features.AI.DTOs;

public sealed record ConfirmAiActionRequest
{
    public required string ConfirmationToken { get; init; }
}