using FluentValidation;

namespace SchoolERP.Application.Features.AI.Commands.RunAgent;

public sealed class RunAiAgentCommandValidator
    : AbstractValidator<RunAiAgentCommand>
{
    public RunAiAgentCommandValidator()
    {
        RuleFor(x => x.Request.Model)
            .NotEmpty();

        RuleFor(x => x.Request.Messages)
            .NotEmpty();

        RuleFor(x => x.Request.MaxSteps)
            .GreaterThan(0)
            .LessThanOrEqualTo(20);
    }
}