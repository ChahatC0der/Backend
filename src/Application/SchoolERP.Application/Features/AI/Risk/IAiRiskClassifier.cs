using SchoolERP.Application.Features.AI.Tools;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.AI.Risk;

public interface IAiRiskClassifier
{
    Result<AiRiskAssessment> Classify(
        ToolDefinition tool);
}