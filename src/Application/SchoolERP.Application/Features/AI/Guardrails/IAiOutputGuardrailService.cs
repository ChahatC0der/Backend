namespace SchoolERP.Application.Features.AI.Guardrails;

public interface IAiOutputGuardrailService
{
    Task<AiOutputGuardrailResult> EvaluateAsync(
        string content,
        CancellationToken cancellationToken = default);
}