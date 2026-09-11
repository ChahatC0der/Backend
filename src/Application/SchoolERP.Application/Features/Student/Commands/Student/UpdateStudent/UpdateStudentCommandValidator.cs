using FluentValidation;

namespace SchoolERP.Application.Features.Student.Commands.Student.UpdateStudent;

public class UpdateStudentCommandValidator : AbstractValidator<UpdateStudentCommand>
{
    public UpdateStudentCommandValidator()
    {
        RuleFor(x => x.Request.Id).GreaterThan(0);
        RuleFor(x => x.Request.FirstName).NotEmpty().MaximumLength(255);
        RuleFor(x => x.Request.Status)
            .Must(s => new[] { "registered", "enrolled", "active", "graduated", "dropped", "blocked", "transferred_out" }.Contains(s));
    }
}
