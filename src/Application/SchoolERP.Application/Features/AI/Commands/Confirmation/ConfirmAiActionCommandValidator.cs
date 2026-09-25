using FluentValidation;

namespace SchoolERP.Application.Features.AI.Commands.Confirmation;

public sealed class ConfirmAiActionCommandValidator
    : AbstractValidator<ConfirmAiActionCommand>
{
    public ConfirmAiActionCommandValidator()
    {
        RuleFor(x => x.ConfirmationToken)
            .NotEmpty()
            .WithMessage(
                "Confirmation token is required.");
    }
}