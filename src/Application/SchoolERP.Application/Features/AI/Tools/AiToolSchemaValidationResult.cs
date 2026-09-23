namespace SchoolERP.Application.Features.AI.Tools;

public sealed record AiToolSchemaValidationResult
{
    public bool IsValid { get; init; }

    public IReadOnlyCollection<string> Errors { get; init; }
        = [];

    public static AiToolSchemaValidationResult Success()
    {
        return new AiToolSchemaValidationResult
        {
            IsValid = true
        };
    }

    public static AiToolSchemaValidationResult Failure(
        IEnumerable<string> errors)
    {
        return new AiToolSchemaValidationResult
        {
            IsValid = false,
            Errors = errors.ToArray()
        };
    }
}