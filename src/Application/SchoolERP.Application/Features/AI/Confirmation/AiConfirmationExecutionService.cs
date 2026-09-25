using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.AI.DTOs;
using SchoolERP.Application.Features.AI.Risk;
using SchoolERP.Application.Features.AI.Services;
using SchoolERP.Application.Features.AI.Tools;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.AI.Confirmation;

public sealed class AiConfirmationExecutionService
    : IAiConfirmationExecutionService
{
    private readonly IAiConfirmationTokenService _tokenService;
    private readonly IAiConfirmationTokenStore _tokenStore;
    private readonly IAiExecutionContextAccessor _executionContextAccessor;
    private readonly IAiToolRegistry _toolRegistry;
    private readonly AiToolAuthorizationService _authorizationService;
    private readonly IAiRiskClassifier _riskClassifier;
    private readonly IEnumerable<IAiToolHandler> _handlers;

    public AiConfirmationExecutionService(
        IAiConfirmationTokenService tokenService,
        IAiConfirmationTokenStore tokenStore,
        IAiExecutionContextAccessor executionContextAccessor,
        IAiToolRegistry toolRegistry,
        AiToolAuthorizationService authorizationService,
        IAiRiskClassifier riskClassifier,
        IEnumerable<IAiToolHandler> handlers)
    {
        _tokenService = tokenService;
        _tokenStore = tokenStore;
        _executionContextAccessor = executionContextAccessor;
        _toolRegistry = toolRegistry;
        _authorizationService = authorizationService;
        _riskClassifier = riskClassifier;
        _handlers = handlers;
    }

    public async Task<Result<AiToolExecutionResult>> ExecuteAsync(
        string confirmationToken,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var tokenResult =
            _tokenService.ReadToken(
                confirmationToken);

        if (tokenResult.IsFailure)
        {
            return Result.Failure<AiToolExecutionResult>(
                tokenResult.Error);
        }

        var pendingAction =
            tokenResult.Value!;

        var currentContext =
            _executionContextAccessor.GetCurrent();

        if (!currentContext.IsAuthenticated)
        {
            return Result.Failure<AiToolExecutionResult>(
                Error.Unauthorized(
                    "Authenticated user context is required to confirm an AI action."));
        }

        if (!currentContext.UserId.HasValue)
        {
            return Result.Failure<AiToolExecutionResult>(
                Error.Unauthorized(
                    "Authenticated user identity could not be established."));
        }

        if (!currentContext.TenantId.HasValue)
        {
            return Result.Failure<AiToolExecutionResult>(
                Error.Unauthorized(
                    "Tenant context could not be established."));
        }

        if (currentContext.UserId.Value != pendingAction.UserId)
        {
            return Result.Failure<AiToolExecutionResult>(
                Error.Unauthorized(
                    "AI confirmation does not belong to the current user."));
        }

        if (currentContext.TenantId.Value != pendingAction.TenantId)
        {
            return Result.Failure<AiToolExecutionResult>(
                Error.Unauthorized(
                    "AI confirmation does not belong to the current tenant."));
        }

        if (currentContext.BranchId != pendingAction.BranchId)
        {
            return Result.Failure<AiToolExecutionResult>(
                Error.Unauthorized(
                    "AI confirmation does not belong to the current branch context."));
        }

        var tool =
            _toolRegistry.Get(
                pendingAction.ToolName,
                pendingAction.Version);

        if (tool is null)
        {
            return Result.Failure<AiToolExecutionResult>(
                Error.NotFound(
                    "AI tool",
                    $"{pendingAction.ToolName}:v{pendingAction.Version}"));
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

        var currentRisk =
            riskResult.Value!;

        if (!currentRisk.RequiresConfirmation)
        {
            return Result.Failure<AiToolExecutionResult>(
                Error.Unauthorized(
                    "AI confirmation is no longer valid because the current risk policy does not require confirmation."));
        }

        if (currentRisk.RiskLevel != pendingAction.RiskLevel)
        {
            return Result.Failure<AiToolExecutionResult>(
                Error.Unauthorized(
                    "AI confirmation no longer matches the current risk policy."));
        }

        var handler =
            _handlers.FirstOrDefault(
                x =>
                    string.Equals(
                        x.Name,
                        pendingAction.ToolName,
                        StringComparison.OrdinalIgnoreCase)
                    && x.Version == pendingAction.Version);

        if (handler is null)
        {
            return Result.Failure<AiToolExecutionResult>(
                Error.Validation(
                    $"No executor is registered for tool '{pendingAction.ToolName}' version {pendingAction.Version}."));
        }

        cancellationToken.ThrowIfCancellationRequested();

        var consumed =
            await _tokenStore.TryConsumeAsync(
                pendingAction.ConfirmationId,
                pendingAction.ExpiresAtUtc,
                cancellationToken);

        if (!consumed)
        {
            return Result.Failure<AiToolExecutionResult>(
                Error.Unauthorized(
                    "AI confirmation token has already been used or is unavailable."));
        }

        cancellationToken.ThrowIfCancellationRequested();

        var executionResult =
            await handler.ExecuteAsync(
                pendingAction.Arguments,
                cancellationToken);

        if (executionResult.IsFailure)
        {
            return executionResult;
        }

        var toolResult =
            executionResult.Value!;

        if (!string.Equals(
                toolResult.ToolName,
                pendingAction.ToolName,
                StringComparison.OrdinalIgnoreCase))
        {
            return Result.Failure<AiToolExecutionResult>(
                Error.Validation(
                    "AI tool handler returned a result for an unexpected tool."));
        }

        return executionResult;
    }
}