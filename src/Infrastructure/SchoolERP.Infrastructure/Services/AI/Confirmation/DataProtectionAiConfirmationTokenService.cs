using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;
using SchoolERP.Application.Features.AI.Confirmation;
using SchoolERP.Application.Features.AI.DTOs;
using SchoolERP.Application.Features.AI.Risk;
using SchoolERP.Application.Features.AI.Tools;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Infrastructure.Services.AI.Confirmation;

public sealed class DataProtectionAiConfirmationTokenService
    : IAiConfirmationTokenService
{
    private const int SupportedTokenVersion = 1;

    private readonly IDataProtector _protector;
    private readonly AiConfirmationOptions _options;

    public DataProtectionAiConfirmationTokenService(
        IDataProtectionProvider dataProtectionProvider,
        IOptions<AiConfirmationOptions> options)
    {
        _protector =
            dataProtectionProvider.CreateProtector(
                "SchoolERP.AI.ConfirmationToken.v1");

        _options = options.Value;
    }

    public Result<AiPendingAction> CreatePendingAction(
        ToolDefinition tool,
        AiActionProposal action,
        AiExecutionContext context,
        AiRiskAssessment riskAssessment)
    {
        if (tool is null)
        {
            return Result.Failure<AiPendingAction>(
                Error.Validation(
                    "Tool definition is required."));
        }

        if (action is null)
        {
            return Result.Failure<AiPendingAction>(
                Error.Validation(
                    "Action proposal is required."));
        }

        if (riskAssessment is null)
        {
            return Result.Failure<AiPendingAction>(
                Error.Validation(
                    "Risk assessment is required."));
        }

        if (!context.IsAuthenticated)
        {
            return Result.Failure<AiPendingAction>(
                Error.Unauthorized(
                    "Authenticated user context is required for AI confirmation."));
        }

        if (!context.UserId.HasValue)
        {
            return Result.Failure<AiPendingAction>(
                Error.Unauthorized(
                    "Authenticated user identity could not be established."));
        }

        if (!context.TenantId.HasValue)
        {
            return Result.Failure<AiPendingAction>(
                Error.Unauthorized(
                    "Tenant context could not be established."));
        }

        if (!string.Equals(
                tool.Name,
                action.ActionName,
                StringComparison.OrdinalIgnoreCase))
        {
            return Result.Failure<AiPendingAction>(
                Error.Validation(
                    "Tool definition and action name do not match."));
        }

        if (tool.Version != action.Version)
        {
            return Result.Failure<AiPendingAction>(
                Error.Validation(
                    "Tool definition and action version do not match."));
        }

        if (!string.Equals(
                tool.Name,
                riskAssessment.ToolName,
                StringComparison.OrdinalIgnoreCase))
        {
            return Result.Failure<AiPendingAction>(
                Error.Validation(
                    "Tool and risk assessment names do not match."));
        }

        if (tool.Version != riskAssessment.Version)
        {
            return Result.Failure<AiPendingAction>(
                Error.Validation(
                    "Tool and risk assessment versions do not match."));
        }

        if (!riskAssessment.RequiresConfirmation)
        {
            return Result.Failure<AiPendingAction>(
                Error.Validation(
                    "A pending confirmation can only be created for an action that requires confirmation."));
        }

        if (_options.TokenLifetimeSeconds <= 0)
        {
            return Result.Failure<AiPendingAction>(
                Error.Validation(
                    "AI confirmation token lifetime must be greater than zero."));
        }

        var createdAtUtc =
            DateTimeOffset.UtcNow;

        var expiresAtUtc =
            createdAtUtc.AddSeconds(
                _options.TokenLifetimeSeconds);

        var confirmationId =
            Guid.NewGuid();

        var payload =
            new AiConfirmationTokenPayload
            {
                TokenVersion = SupportedTokenVersion,
                ConfirmationId = confirmationId,
                ToolName = tool.Name,
                Version = tool.Version,
                RiskLevel = riskAssessment.RiskLevel,
                Arguments = action.Arguments,
                UserId = context.UserId.Value,
                TenantId = context.TenantId.Value,
                BranchId = context.BranchId,
                CreatedAtUtc = createdAtUtc,
                ExpiresAtUtc = expiresAtUtc
            };

        var json =
            JsonSerializer.Serialize(payload);

        string protectedToken;

        try
        {
            protectedToken =
                _protector.Protect(json);
        }
        catch (Exception ex)
            when (ex is CryptographicException
                  or ArgumentException)
        {
            return Result.Failure<AiPendingAction>(
                Error.Validation(
                    "AI confirmation token could not be created."));
        }

        return Result.Success(
            new AiPendingAction
            {
                TokenVersion = SupportedTokenVersion,
                ConfirmationId = confirmationId,
                ConfirmationToken = protectedToken,
                ToolName = tool.Name,
                Version = tool.Version,
                RiskLevel = riskAssessment.RiskLevel,
                Arguments = action.Arguments,
                UserId = context.UserId.Value,
                TenantId = context.TenantId.Value,
                BranchId = context.BranchId,
                CreatedAtUtc = createdAtUtc,
                ExpiresAtUtc = expiresAtUtc
            });
    }

    public Result<AiPendingAction> ReadToken(
        string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return Result.Failure<AiPendingAction>(
                Error.Unauthorized(
                    "AI confirmation token is required."));
        }

        AiConfirmationTokenPayload payload;

        try
        {
            var json =
                _protector.Unprotect(token);

            payload =
                JsonSerializer.Deserialize<AiConfirmationTokenPayload>(
                    json)!;
        }
        catch (Exception ex)
            when (ex is CryptographicException
                  or JsonException
                  or ArgumentException
                  or NotSupportedException)
        {
            return Result.Failure<AiPendingAction>(
                Error.Unauthorized(
                    "Invalid or tampered AI confirmation token."));
        }

        if (payload is null)
        {
            return Result.Failure<AiPendingAction>(
                Error.Unauthorized(
                    "Invalid AI confirmation token."));
        }

        if (payload.TokenVersion != SupportedTokenVersion)
        {
            return Result.Failure<AiPendingAction>(
                Error.Unauthorized(
                    "Unsupported AI confirmation token version."));
        }

        if (payload.ConfirmationId == Guid.Empty)
        {
            return Result.Failure<AiPendingAction>(
                Error.Unauthorized(
                    "Invalid AI confirmation token."));
        }

        if (string.IsNullOrWhiteSpace(payload.ToolName))
        {
            return Result.Failure<AiPendingAction>(
                Error.Unauthorized(
                    "Invalid AI confirmation token."));
        }

        if (payload.Version <= 0)
        {
            return Result.Failure<AiPendingAction>(
                Error.Unauthorized(
                    "Invalid AI confirmation token."));
        }

        if (payload.UserId == Guid.Empty
            || payload.TenantId == Guid.Empty)
        {
            return Result.Failure<AiPendingAction>(
                Error.Unauthorized(
                    "Invalid AI confirmation token."));
        }

        if (payload.ExpiresAtUtc <= payload.CreatedAtUtc)
        {
            return Result.Failure<AiPendingAction>(
                Error.Unauthorized(
                    "Invalid AI confirmation token lifetime."));
        }

        if (payload.ExpiresAtUtc <= DateTimeOffset.UtcNow)
        {
            return Result.Failure<AiPendingAction>(
                Error.Unauthorized(
                    "AI confirmation token has expired."));
        }

        return Result.Success(
            new AiPendingAction
            {
                TokenVersion = payload.TokenVersion,
                ConfirmationId = payload.ConfirmationId,
                ConfirmationToken = token,
                ToolName = payload.ToolName,
                Version = payload.Version,
                RiskLevel = payload.RiskLevel,
                Arguments = payload.Arguments
                    ?? new Dictionary<string, JsonElement>(),
                UserId = payload.UserId,
                TenantId = payload.TenantId,
                BranchId = payload.BranchId,
                CreatedAtUtc = payload.CreatedAtUtc,
                ExpiresAtUtc = payload.ExpiresAtUtc
            });
    }

    private sealed record AiConfirmationTokenPayload
    {
        public int TokenVersion { get; init; }

        public Guid ConfirmationId { get; init; }

        public required string ToolName { get; init; }

        public int Version { get; init; }

        public AiRiskLevel RiskLevel { get; init; }

        public IReadOnlyDictionary<string, JsonElement>? Arguments { get; init; }

        public Guid UserId { get; init; }

        public Guid TenantId { get; init; }

        public Guid? BranchId { get; init; }

        public DateTimeOffset CreatedAtUtc { get; init; }

        public DateTimeOffset ExpiresAtUtc { get; init; }
    }
}