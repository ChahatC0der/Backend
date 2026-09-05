using FluentValidation;

namespace SchoolERP.Application.Features.Academic.Commands.ClassGroup.CreateClassGroup;

public class CreateClassGroupCommandValidator : AbstractValidator<CreateClassGroupCommand>
{
    public CreateClassGroupCommandValidator()
    {
        RuleFor(x => x.Request.Name).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Request.Sequence).GreaterThan((byte)0);
        RuleFor(x => x.Request.Description).MaximumLength(255).When(x => x.Request.Description != null);
    }
}