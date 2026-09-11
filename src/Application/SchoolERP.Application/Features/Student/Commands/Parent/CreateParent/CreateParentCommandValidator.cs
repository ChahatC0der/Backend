using FluentValidation;

namespace SchoolERP.Application.Features.Student.Commands.Parent.CreateParent;

public class CreateParentCommandValidator : AbstractValidator<CreateParentCommand>
{
    public CreateParentCommandValidator()
    {
        RuleFor(x => x.Request.StudentId).GreaterThan(0);
        RuleFor(x => x.Request.ParentType).NotEmpty()
            .Must(t => new[] { "father", "mother", "guardian", "other" }.Contains(t));
        RuleFor(x => x.Request.FirstName).NotEmpty().MaximumLength(255);
        RuleFor(x => x.Request.Email).EmailAddress().When(x => !string.IsNullOrEmpty(x.Request.Email));
        RuleFor(x => x.Request.Mobile).MaximumLength(20).When(x => !string.IsNullOrEmpty(x.Request.Mobile));
    }
}
