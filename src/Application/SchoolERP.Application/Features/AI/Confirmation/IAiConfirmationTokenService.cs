using SchoolERP.Application.Features.AI.DTOs;
using SchoolERP.Application.Features.AI.Risk;
using SchoolERP.Application.Features.AI.Tools;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.AI.Confirmation;

public interface IAiConfirmationTokenService
{
    Result<AiPendingAction> CreatePendingAction(
        ToolDefinition tool,
        AiActionProposal action,
        AiExecutionContext context,
        AiRiskAssessment riskAssessment);

    Result<AiPendingAction> ReadToken(
        string token);
}