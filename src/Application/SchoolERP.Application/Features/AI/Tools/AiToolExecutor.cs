using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.AI.Confirmation;
using SchoolERP.Application.Features.AI.DTOs;
using SchoolERP.Application.Features.AI.Risk;
using SchoolERP.Application.Features.AI.Services;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.AI.Tools;

public sealed class AiToolExecutor : IAiToolExecutor
{
    private readonly IEnumerable<IAiToolHandler> _handlers;
    private readonly AiToolAuthorizationService _authorizationService;
    private readonly IAiRiskClassifier _riskClassifier;
    private readonly IAiExecutionContextAccessor _executionContextAccessor;
    private readonly IAiConfirmationTokenService _confirmationTokenService;

    public AiToolExecutor(
        IEnumerable<IAiToolHandler> handlers,
        AiToolAuthorizationService authorizationService,
        IAiRiskClassifier riskClassifier,
        IAiExecutionContextAccessor executionContextAccessor,
        IAiConfirmationTokenService confirmationTokenService)
    {
        _handlers = handlers;
        _authorizationService = authorizationService;
        _riskClassifier = riskClassifier;
        _executionContextAccessor = executionContextAccessor;
        _confirmationTokenService = confirmationTokenService;
    }

    public async Task<Result<AiToolExecutionResult>> ExecuteAsync(
        ToolDefinition tool,
        AiActionProposal action,
        CancellationToken cancellationToken = default)
    {
        if (tool is null)
        {
            return Result.Failure<AiToolExecutionResult>(
                Error.Validation(
                    "Tool definition is required."));
        }

        if (action is null)
        {
            return Result.Failure<AiToolExecutionResult>(
                Error.Validation(
                    "Action proposal is required."));
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

        var authorizationResult =
            _authorizationService.Authorize(tool);

        if (!authorizationResult.IsAllowed)
        {
            return Result.Failure<AiToolExecutionResult>(
                Error.Unauthorized(
                    string.Join(
                        " | ",
                        authorizationResult.Errors)));
        }

        var riskResult =
            _riskClassifier.Classify(tool);

        if (riskResult.IsFailure)
        {
            return Result.Failure<AiToolExecutionResult>(
                riskResult.Error);
        }

        var riskAssessment =
            riskResult.Value!;

        if (riskAssessment.RequiresConfirmation)
        {
            var currentContext =
                _executionContextAccessor.GetCurrent();

            var pendingActionResult =
                _confirmationTokenService.CreatePendingAction(
                    tool,
                    action,
                    currentContext,
                    riskAssessment);

            if (pendingActionResult.IsFailure)
            {
                return Result.Failure<AiToolExecutionResult>(
                    pendingActionResult.Error);
            }

            var pendingAction =
                pendingActionResult.Value!;

            var reasons =
                riskAssessment.Reasons.Count > 0
                    ? riskAssessment.Reasons
                    : [
                        "Explicit confirmation is required before executing this AI tool."
                    ];

            var confirmationRequirement =
                new AiConfirmationRequirement
                {
                    ConfirmationId =
                        pendingAction.ConfirmationId,

                    ConfirmationToken =
                        pendingAction.ConfirmationToken,

                    ToolName =
                        pendingAction.ToolName,

                    Version =
                        pendingAction.Version,

                    RiskLevel =
                        pendingAction.RiskLevel,

                    ExpiresAtUtc =
                        pendingAction.ExpiresAtUtc,

                    Reasons =
                        reasons
                };

            return Result.Success(
                AiToolExecutionResult.ConfirmationRequired(
                    confirmationRequirement));
        }

        var handler =
            _handlers.FirstOrDefault(
                x =>
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

        cancellationToken.ThrowIfCancellationRequested();

        return await handler.ExecuteAsync(
            action.Arguments,
            cancellationToken);
    }
}