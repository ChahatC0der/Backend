using System.Text.RegularExpressions;
using SchoolERP.Application.Features.AI.Guardrails;

namespace SchoolERP.Infrastructure.Services.AI.Guardrails;

public sealed partial class AiSensitiveOutputGuardrail
    : IAiOutputGuardrail
{
    private static readonly string[] SensitivePatterns =
    [
        "reveal system prompt",
        "show system prompt",
        "print system prompt",
        "system prompt is",
        "my system prompt",
        "system instructions are",
        "reveal developer prompt",
        "show developer prompt",
        "developer instructions are",

        "api key:",
        "api_key:",
        "apikey:",
        "secret key:",
        "client secret:",
        "connection string:",
        "password=",
        "password:",
        "authorization: bearer",
        "bearer ey",

        "ignore previous instructions",
        "ignore all previous instructions",
        "disregard previous instructions",
        "reveal hidden instructions"
    ];

    public Task<AiOutputGuardrailResult> EvaluateAsync(
        string content,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(content))
        {
            return Task.FromResult(
                AiOutputGuardrailResult.Blocked(
                    "AI response content cannot be empty."));
        }

        var normalized =
            Normalize(content);

        foreach (var pattern in SensitivePatterns)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var normalizedPattern =
                Normalize(pattern);

            if (normalized.Contains(
                    normalizedPattern,
                    StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult(
                    AiOutputGuardrailResult.Blocked(
                        $"Potential sensitive AI output detected: '{pattern}'."));
            }
        }

        return Task.FromResult(
            AiOutputGuardrailResult.Allowed());
    }

    private static string Normalize(
        string content)
    {
        return WhitespaceRegex()
            .Replace(
                content
                    .Trim()
                    .ToLowerInvariant(),
                " ");
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();
}