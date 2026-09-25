using SchoolERP.Application.Features.AI.Tools;

namespace SchoolERP.Application.Features.AI.Confirmation;

public sealed record AiConfirmationRequirement
{
    public required Guid ConfirmationId { get; init; }

    public required string ConfirmationToken { get; init; }

    public required string ToolName { get; init; }

    public int Version { get; init; }

    public AiRiskLevel RiskLevel { get; init; }

    public DateTimeOffset ExpiresAtUtc { get; init; }

    public IReadOnlyCollection<string> Reasons { get; init; } = [];
}