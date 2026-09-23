using SchoolERP.Application.Features.AI.DTOs;
using SchoolERP.Application.Features.AI.Tools;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Common.Interfaces;

public interface IAiToolExecutor
{
    Task<Result<AiToolExecutionResult>> ExecuteAsync(
        ToolDefinition tool,
        AiActionProposal action,
        CancellationToken cancellationToken = default);
}