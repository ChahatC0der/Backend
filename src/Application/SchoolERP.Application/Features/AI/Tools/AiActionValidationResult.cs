namespace SchoolERP.Application.Features.AI.Tools;

public sealed record AiActionValidationResult
{
    public bool IsValid { get; init; }

    public ToolDefinition? Tool { get; init; }

    public IReadOnlyCollection<string> Errors { get; init; }
        = [];

    public static AiActionValidationResult Success(
        ToolDefinition tool)
    {
        return new AiActionValidationResult
        {
            IsValid = true,
            Tool = tool
        };
    }

    public static AiActionValidationResult Failure(
        IEnumerable<string> errors)
    {
        return new AiActionValidationResult
        {
            IsValid = false,
            Errors = errors.ToArray()
        };
    }
}