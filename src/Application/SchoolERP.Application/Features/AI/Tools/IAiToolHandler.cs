using System.Text.Json;
using SchoolERP.Application.Features.AI.DTOs;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.AI.Tools;

public interface IAiToolHandler
{
    string Name { get; }

    int Version { get; }

    Task<Result<AiToolExecutionResult>> ExecuteAsync(
        IReadOnlyDictionary<string, JsonElement> arguments,
        CancellationToken cancellationToken = default);
}