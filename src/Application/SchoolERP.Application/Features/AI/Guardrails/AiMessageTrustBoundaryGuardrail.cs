using SchoolERP.Application.Features.AI.DTOs;
using SchoolERP.Application.Features.AI.Guardrails;

namespace SchoolERP.Infrastructure.Services.AI.Guardrails;

public sealed class AiMessageTrustBoundaryGuardrail
    : IAiInputGuardrail
{
    public Task<AiGuardrailResult> EvaluateAsync(
        IReadOnlyList<AiMessage> messages,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (messages is null || messages.Count == 0)
        {
            return Task.FromResult(
                AiGuardrailResult.Blocked(
                    "AI message list cannot be empty."));
        }

        foreach (var message in messages)
        {
            cancellationToken.ThrowIfCancellationRequested();

            switch (message.Role)
            {
                case AiMessageRole.System:
                    return Task.FromResult(
                        AiGuardrailResult.Blocked(
                            "System messages are server-controlled and cannot be supplied by the client."));

                case AiMessageRole.Tool:
                    return Task.FromResult(
                        AiGuardrailResult.Blocked(
                            "Tool messages are application-controlled and cannot be supplied by the client."));
            }
        }

        return Task.FromResult(
            AiGuardrailResult.Allowed());
    }
}