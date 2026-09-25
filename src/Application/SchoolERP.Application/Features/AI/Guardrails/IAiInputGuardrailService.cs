using SchoolERP.Application.Features.AI.DTOs;

namespace SchoolERP.Application.Features.AI.Guardrails;

public interface IAiInputGuardrailService
{
    Task<AiGuardrailResult> EvaluateAsync(
        IReadOnlyList<AiMessage> messages,
        CancellationToken cancellationToken = default);
}