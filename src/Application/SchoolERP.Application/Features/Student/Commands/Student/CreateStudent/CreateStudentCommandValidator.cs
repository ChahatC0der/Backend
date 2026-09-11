using FluentValidation;

namespace SchoolERP.Application.Features.Student.Commands.Student.CreateStudent;

public class CreateStudentCommandValidator : AbstractValidator<CreateStudentCommand>
{
    public CreateStudentCommandValidator()
    {
        RuleFor(x => x.Request.FirstName).NotEmpty().MaximumLength(255);
        RuleFor(x => x.Request.LastName).MaximumLength(255);
        RuleFor(x => x.Request.DateOfBirth).NotEmpty();
        RuleFor(x => x.Request.AdmissionDate).NotEmpty();
        RuleFor(x => x.Request.Status)
            .Must(s => new[] { "registered", "enrolled", "active", "graduated", "dropped", "blocked", "transferred_out" }.Contains(s));
    }
}
