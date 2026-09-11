using FluentValidation;

namespace SchoolERP.Application.Features.Student.Commands.Parent.PatchParent;

public class PatchParentCommandValidator : AbstractValidator<PatchParentCommand>
{
    public PatchParentCommandValidator()
    {
        RuleFor(x => x.Request.Id).GreaterThan(0);
        RuleFor(x => x.Request.ParentType)
            .Must(t => new[] { "father", "mother", "guardian", "other" }.Contains(t))
            .When(x => !string.IsNullOrEmpty(x.Request.ParentType));
        RuleFor(x => x.Request.FirstName).MaximumLength(255).When(x => !string.IsNullOrEmpty(x.Request.FirstName));
    }
}
