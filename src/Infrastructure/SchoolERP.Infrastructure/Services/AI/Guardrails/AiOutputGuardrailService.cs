using SchoolERP.Application.Features.AI.Guardrails;

namespace SchoolERP.Infrastructure.Services.AI.Guardrails;

public sealed class AiOutputGuardrailService
    : IAiOutputGuardrailService
{
    private readonly IEnumerable<IAiOutputGuardrail> _guardrails;

    public AiOutputGuardrailService(
        IEnumerable<IAiOutputGuardrail> guardrails)
    {
        _guardrails = guardrails;
    }

    public async Task<AiOutputGuardrailResult> EvaluateAsync(
        string content,
        CancellationToken cancellationToken = default)
    {
        if (content is null)
        {
            return AiOutputGuardrailResult.Blocked(
                "AI response content cannot be null.");
        }

        foreach (var guardrail in _guardrails)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var result =
                await guardrail.EvaluateAsync(
                    content,
                    cancellationToken);

            if (!result.IsAllowed)
                return result;
        }

        return AiOutputGuardrailResult.Allowed();
    }
}