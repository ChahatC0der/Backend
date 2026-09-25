using SchoolERP.Application.Features.AI.DTOs;

namespace SchoolERP.Application.Features.AI.Guardrails;

public sealed class AiInputGuardrailService
    : IAiInputGuardrailService
{
    private readonly IEnumerable<IAiInputGuardrail> _guardrails;

    public AiInputGuardrailService(
        IEnumerable<IAiInputGuardrail> guardrails)
    {
        _guardrails = guardrails;
    }

    public async Task<AiGuardrailResult> EvaluateAsync(
        IReadOnlyList<AiMessage> messages,
        CancellationToken cancellationToken = default)
    {
        if (messages is null || messages.Count == 0)
        {
            return AiGuardrailResult.Blocked(
                "AI message list cannot be empty.");
        }

        foreach (var guardrail in _guardrails)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var result = await guardrail.EvaluateAsync(
                messages,
                cancellationToken);

            if (!result.IsAllowed)
            {
                return result;
            }
        }

        return AiGuardrailResult.Allowed();
    }
}