using System.Text.Json;
using SchoolERP.Application.Features.AI.Tools;

namespace SchoolERP.Application.Features.AI.Confirmation;

public sealed record AiPendingAction
{
    public int TokenVersion { get; init; } = 1;

    public required Guid ConfirmationId { get; init; }

    public required string ConfirmationToken { get; init; }

    public required string ToolName { get; init; }

    public int Version { get; init; }

    public AiRiskLevel RiskLevel { get; init; }

    public required IReadOnlyDictionary<string, JsonElement> Arguments { get; init; }

    public required Guid UserId { get; init; }

    public required Guid TenantId { get; init; }

    public Guid? BranchId { get; init; }

    public DateTimeOffset CreatedAtUtc { get; init; }

    public DateTimeOffset ExpiresAtUtc { get; init; }
}