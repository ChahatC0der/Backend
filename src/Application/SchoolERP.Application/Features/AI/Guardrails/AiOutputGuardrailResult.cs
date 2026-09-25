namespace SchoolERP.Application.Features.AI.Guardrails;

public sealed record AiOutputGuardrailResult
{
    public bool IsAllowed { get; init; }

    public IReadOnlyCollection<string> Reasons { get; init; } = [];

    public static AiOutputGuardrailResult Allowed()
        => new()
        {
            IsAllowed = true
        };

    public static AiOutputGuardrailResult Blocked(
        params string[] reasons)
        => new()
        {
            IsAllowed = false,
            Reasons = reasons
        };
}