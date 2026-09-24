namespace SchoolERP.Application.Features.AI.Tools;

public sealed record AiToolAuthorizationResult
{
    public bool IsAllowed { get; init; }

    public IReadOnlyCollection<string> Errors { get; init; } = [];

    public static AiToolAuthorizationResult Allowed()
        => new()
        {
            IsAllowed = true
        };

    public static AiToolAuthorizationResult Denied(
        params string[] errors)
        => new()
        {
            IsAllowed = false,
            Errors = errors
        };
}