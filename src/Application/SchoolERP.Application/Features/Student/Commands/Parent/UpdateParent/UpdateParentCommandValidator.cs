using FluentValidation;

namespace SchoolERP.Application.Features.Student.Commands.Parent.UpdateParent;

public class UpdateParentCommandValidator : AbstractValidator<UpdateParentCommand>
{
    public UpdateParentCommandValidator()
    {
        RuleFor(x => x.Request.Id).GreaterThan(0);
        RuleFor(x => x.Request.StudentId).GreaterThan(0);
        RuleFor(x => x.Request.ParentType).NotEmpty()
            .Must(t => new[] { "father", "mother", "guardian", "other" }.Contains(t));
        RuleFor(x => x.Request.FirstName).NotEmpty().MaximumLength(255);
    }
}
