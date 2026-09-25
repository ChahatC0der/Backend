namespace SchoolERP.Application.Features.AI.Confirmation;

public sealed class AiConfirmationOptions
{
    public const string SectionName = "AI:Confirmation";

    public int TokenLifetimeSeconds { get; init; } = 300;
}