using System.Text.Json;

namespace SchoolERP.Application.Features.AI.Streaming.DTOs;

public sealed record AiStreamToolProposedData
{
    public required string ActionName { get; init; }

    public int Version { get; init; }

    public IReadOnlyDictionary<string, JsonElement> Arguments { get; init; }
        = new Dictionary<string, JsonElement>();
}       