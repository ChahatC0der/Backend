using System.Text.Json;

namespace SchoolERP.Application.Features.AI.Streaming;

public sealed record AiStreamEvent
{
    public required AiStreamEventType Type { get; init; }

    public string? Content { get; init; }

    public JsonElement? Data { get; init; }

    public DateTimeOffset TimestampUtc { get; init; } =
        DateTimeOffset.UtcNow;
}