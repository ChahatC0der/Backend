using System.Text.Json;

namespace SchoolERP.Application.Features.AI.Streaming.DTOs;

public sealed record AiStreamErrorData
{
    public string? Message { get; init; }

    public JsonElement? Details { get; init; }
}