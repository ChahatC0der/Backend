using SchoolERP.Application.Features.AI.DTOs;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.AI.Confirmation;

public interface IAiConfirmationExecutionService
{
    Task<Result<AiToolExecutionResult>> ExecuteAsync(
        string confirmationToken,
        CancellationToken cancellationToken = default);
}