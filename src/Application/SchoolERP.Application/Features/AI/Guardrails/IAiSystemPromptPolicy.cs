using SchoolERP.Application.Features.AI.DTOs;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.AI.Guardrails;

public interface IAiSystemPromptPolicy
{
    Result<AiChatRequest> Apply(
        AiChatRequest request);
}