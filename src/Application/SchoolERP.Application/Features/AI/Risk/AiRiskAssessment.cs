using SchoolERP.Application.Features.AI.Tools;

namespace SchoolERP.Application.Features.AI.Risk;

public sealed record AiRiskAssessment
{
    public required string ToolName { get; init; }

    public int Version { get; init; }

    public AiRiskLevel RiskLevel { get; init; }

    public bool RequiresConfirmation { get; init; }

    public bool CanExecuteImmediately =>
        !RequiresConfirmation;

    public IReadOnlyCollection<string> Reasons { get; init; } = [];
}