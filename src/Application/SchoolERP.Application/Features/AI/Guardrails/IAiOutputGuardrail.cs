namespace SchoolERP.Application.Features.AI.Guardrails;

public interface IAiOutputGuardrail
{
    Task<AiOutputGuardrailResult> EvaluateAsync(
        string content,
        CancellationToken cancellationToken = default);
}