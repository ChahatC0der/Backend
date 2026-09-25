using SchoolERP.Application.Features.AI.Tools;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.AI.Risk;

public sealed class AiRiskClassifier
    : IAiRiskClassifier
{
    public Result<AiRiskAssessment> Classify(
        ToolDefinition tool)
    {
        if (tool is null)
        {
            return Result.Failure<AiRiskAssessment>(
                Error.Validation(
                    "Tool definition is required."));
        }

        if (string.IsNullOrWhiteSpace(tool.Name))
        {
            return Result.Failure<AiRiskAssessment>(
                Error.Validation(
                    "Tool name is required."));
        }

        if (tool.Version <= 0)
        {
            return Result.Failure<AiRiskAssessment>(
                Error.Validation(
                    "Tool version must be greater than zero."));
        }

        var reasons =
            new List<string>();

        var requiresConfirmation =
            tool.ConfirmationPolicy ==
                AiConfirmationPolicy.Required;

        switch (tool.RiskLevel)
        {
            case AiRiskLevel.Low:
                break;

            case AiRiskLevel.Medium:
                if (requiresConfirmation)
                {
                    reasons.Add(
                        "Tool confirmation policy requires explicit confirmation.");
                }

                break;

            case AiRiskLevel.High:
                requiresConfirmation = true;

                reasons.Add(
                    "High-risk AI actions require explicit confirmation.");

                break;

            case AiRiskLevel.Critical:
                requiresConfirmation = true;

                reasons.Add(
                    "Critical-risk AI actions always require explicit confirmation.");

                break;

            default:
                return Result.Failure<AiRiskAssessment>(
                    Error.Validation(
                        $"Unsupported AI risk level '{tool.RiskLevel}'."));
        }

        if (tool.ConfirmationPolicy ==
            AiConfirmationPolicy.Required
            &&
            !reasons.Any(
                x =>
                    x.Contains(
                        "confirmation policy",
                        StringComparison.OrdinalIgnoreCase)))
        {
            reasons.Add(
                "Tool confirmation policy requires explicit confirmation.");
        }

        return Result.Success(
            new AiRiskAssessment
            {
                ToolName = tool.Name,
                Version = tool.Version,
                RiskLevel = tool.RiskLevel,
                RequiresConfirmation = requiresConfirmation,
                Reasons = reasons
            });
    }
}