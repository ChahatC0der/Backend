using FluentValidation;

namespace SchoolERP.Application.Features.Student.Commands.Alumni.ConvertStudentToAlumni;

public class ConvertStudentToAlumniCommandValidator : AbstractValidator<ConvertStudentToAlumniCommand>
{
    public ConvertStudentToAlumniCommandValidator()
    {
        RuleFor(x => x.Request.StudentId).GreaterThan(0);
        RuleFor(x => x.Request.GraduationDate).NotEmpty();
        RuleFor(x => x.Request.LastClassId).GreaterThan(0);
        RuleFor(x => x.Request.PassingAcademicYearId).GreaterThan(0);
    }
}
