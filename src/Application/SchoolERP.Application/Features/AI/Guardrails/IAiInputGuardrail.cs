using SchoolERP.Application.Features.AI.DTOs;

namespace SchoolERP.Application.Features.AI.Guardrails;

public interface IAiInputGuardrail
{
    Task<AiGuardrailResult> EvaluateAsync(
        IReadOnlyList<AiMessage> messages,
        CancellationToken cancellationToken = default);
}