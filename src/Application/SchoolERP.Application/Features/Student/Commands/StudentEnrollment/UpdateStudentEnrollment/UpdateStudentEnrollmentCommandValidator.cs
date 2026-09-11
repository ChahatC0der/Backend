using FluentValidation;

namespace SchoolERP.Application.Features.Student.Commands.StudentEnrollment.UpdateStudentEnrollment;

public class UpdateStudentEnrollmentCommandValidator : AbstractValidator<UpdateStudentEnrollmentCommand>
{
    public UpdateStudentEnrollmentCommandValidator()
    {
        RuleFor(x => x.Request.Id).GreaterThan(0);
        RuleFor(x => x.Request.AcademicYearId).GreaterThan(0);
        RuleFor(x => x.Request.ClassId).GreaterThan(0);
        RuleFor(x => x.Request.SectionId).GreaterThan(0);
    }
}
