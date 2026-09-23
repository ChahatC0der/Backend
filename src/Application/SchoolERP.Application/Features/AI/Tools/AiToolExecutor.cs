using System.Linq;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.AI.DTOs;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.AI.Tools;

public sealed class AiToolExecutor : IAiToolExecutor
{
    private readonly IEnumerable<IAiToolHandler> _handlers;

    public AiToolExecutor(
        IEnumerable<IAiToolHandler> handlers)
    {
        _handlers = handlers;
    }

    public async Task<Result<AiToolExecutionResult>> ExecuteAsync(
        ToolDefinition tool,
        AiActionProposal action,
        CancellationToken cancellationToken = default)
    {
        if (tool is null)
        {
            return Result.Failure<AiToolExecutionResult>(
                Error.Validation("Tool definition is required."));
        }

        if (action is null)
        {
            return Result.Failure<AiToolExecutionResult>(
                Error.Validation("Action proposal is required."));
        }

        if (!string.Equals(
                tool.Name,
                action.ActionName,
                StringComparison.OrdinalIgnoreCase))
        {
            return Result.Failure<AiToolExecutionResult>(
                Error.Validation(
                    "Tool definition and action name do not match."));
        }

        if (tool.Version != action.Version)
        {
            return Result.Failure<AiToolExecutionResult>(
                Error.Validation(
                    "Tool definition and action version do not match."));
        }

        var handler = _handlers.FirstOrDefault(x =>
            string.Equals(
                x.Name,
                tool.Name,
                StringComparison.OrdinalIgnoreCase)
            && x.Version == tool.Version);

        if (handler is null)
        {
            return Result.Failure<AiToolExecutionResult>(
                Error.Validation(
                    $"No executor is registered for tool '{tool.Name}' version {tool.Version}."));
        }

        return await handler.ExecuteAsync(
            action.Arguments,
            cancellationToken);
    }
}