using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using SchoolERP.Application.Features.AI.DTOs;
using SchoolERP.Application.Features.AI.Guardrails;

namespace SchoolERP.Infrastructure.Services.AI.Guardrails;

public sealed partial class PromptInjectionGuardrail
    : IAiInputGuardrail
{
    private static readonly string[] SuspiciousPhrases =
    [
        "ignore previous instructions",
        "ignore all previous instructions",
        "ignore prior instructions",
        "ignore all prior instructions",
        "ignore earlier instructions",
        "ignore all earlier instructions",

        "disregard previous instructions",
        "disregard all previous instructions",
        "disregard prior instructions",
        "disregard all prior instructions",
        "disregard earlier instructions",
        "disregard all earlier instructions",

        "forget previous instructions",
        "forget all previous instructions",
        "forget prior instructions",
        "forget all prior instructions",

        "reveal system prompt",
        "show system prompt",
        "print system prompt",
        "display system prompt",
        "reveal your system prompt",
        "show your system prompt",

        "reveal developer prompt",
        "show developer prompt",
        "reveal developer instructions",
        "show developer instructions",

        "what is your system prompt",
        "what is your developer prompt",
        "what are your system instructions",
        "what are your developer instructions",

        "reveal your instructions",
        "show your instructions",
        "print your instructions",

        "bypass your instructions",
        "override your instructions",
        "override system instructions",
        "bypass system instructions",
        "override security rules",
        "bypass security rules",

        "developer mode",
        "jailbreak",
        "do anything now",
        "dan mode",

        "act as system",
        "act as developer",
        "pretend to be system",
        "pretend to be developer",

        "reveal hidden instructions",
        "show hidden instructions",
        "reveal hidden prompt",
        "show hidden prompt"
    ];

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

            if (string.IsNullOrWhiteSpace(message.Content))
                continue;

            var normalized = Normalize(message.Content);
            var compact = Compact(normalized);

            foreach (var phrase in SuspiciousPhrases)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var normalizedPhrase =
                    Normalize(phrase);

                var compactPhrase =
                    Compact(normalizedPhrase);

                if (normalized.Contains(
                        normalizedPhrase,
                        StringComparison.OrdinalIgnoreCase)
                    ||
                    compact.Contains(
                        compactPhrase,
                        StringComparison.OrdinalIgnoreCase))
                {
                    return Task.FromResult(
                        AiGuardrailResult.Blocked(
                            $"Potential prompt injection detected: '{phrase}'."));
                }
            }
        }

        return Task.FromResult(
            AiGuardrailResult.Allowed());
    }

    private static string Normalize(string content)
    {
        var normalized =
            content.Normalize(
                NormalizationForm.FormKC);

        var builder = new StringBuilder(
            normalized.Length);

        foreach (var character in normalized)
        {
            var category =
                CharUnicodeInfo.GetUnicodeCategory(
                    character);

            if (category == UnicodeCategory.Format
                || category == UnicodeCategory.Control)
            {
                builder.Append(' ');
                continue;
            }

            if (char.IsPunctuation(character)
                || char.IsSymbol(character))
            {
                builder.Append(' ');
                continue;
            }

            builder.Append(
                char.ToLowerInvariant(character));
        }

        return WhitespaceRegex()
            .Replace(
                builder.ToString().Trim(),
                " ");
    }

    private static string Compact(string content)
    {
        return content.Replace(
            " ",
            string.Empty,
            StringComparison.Ordinal);
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();
}