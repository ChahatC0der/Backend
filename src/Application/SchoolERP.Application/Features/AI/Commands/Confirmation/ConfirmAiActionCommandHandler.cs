using MediatR;
using SchoolERP.Application.Features.AI.Confirmation;
using SchoolERP.Application.Features.AI.DTOs;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.AI.Commands.Confirmation;

public sealed class ConfirmAiActionCommandHandler
    : IRequestHandler<
        ConfirmAiActionCommand,
        Result<AiToolExecutionResult>>
{
    private readonly IAiConfirmationExecutionService
        _confirmationExecutionService;

    public ConfirmAiActionCommandHandler(
        IAiConfirmationExecutionService
            confirmationExecutionService)
    {
        _confirmationExecutionService =
            confirmationExecutionService;
    }

    public async Task<Result<AiToolExecutionResult>> Handle(
        ConfirmAiActionCommand request,
        CancellationToken cancellationToken)
    {
        return await _confirmationExecutionService.ExecuteAsync(
            request.ConfirmationToken,
            cancellationToken);
    }
}