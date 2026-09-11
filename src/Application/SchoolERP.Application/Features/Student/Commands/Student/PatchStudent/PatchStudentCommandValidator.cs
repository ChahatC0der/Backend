using FluentValidation;

namespace SchoolERP.Application.Features.Student.Commands.Student.PatchStudent;

public class PatchStudentCommandValidator : AbstractValidator<PatchStudentCommand>
{
    public PatchStudentCommandValidator()
    {
        RuleFor(x => x.Request.Id).GreaterThan(0);
        RuleFor(x => x.Request.FirstName).MaximumLength(255).When(x => !string.IsNullOrEmpty(x.Request.FirstName));
        RuleFor(x => x.Request.Status)
            .Must(s => new[] { "registered", "enrolled", "active", "graduated", "dropped", "blocked", "transferred_out" }.Contains(s))
            .When(x => !string.IsNullOrEmpty(x.Request.Status));
    }
}
