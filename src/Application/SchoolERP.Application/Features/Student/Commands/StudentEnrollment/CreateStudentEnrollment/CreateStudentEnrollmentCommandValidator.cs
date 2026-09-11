using FluentValidation;

namespace SchoolERP.Application.Features.Student.Commands.StudentEnrollment.CreateStudentEnrollment;

public class CreateStudentEnrollmentCommandValidator : AbstractValidator<CreateStudentEnrollmentCommand>
{
    public CreateStudentEnrollmentCommandValidator()
    {
        RuleFor(x => x.Request.StudentId).GreaterThan(0);
        RuleFor(x => x.Request.AcademicYearId).GreaterThan(0);
        RuleFor(x => x.Request.ClassId).GreaterThan(0);
        RuleFor(x => x.Request.SectionId).GreaterThan(0);
        RuleFor(x => x.Request.RollNumber).MaximumLength(20).When(x => !string.IsNullOrEmpty(x.Request.RollNumber));
    }
}
