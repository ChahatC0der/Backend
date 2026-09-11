using FluentValidation;

namespace SchoolERP.Application.Features.Student.Commands.StudentEnrollment.PatchStudentEnrollment;

public class PatchStudentEnrollmentCommandValidator : AbstractValidator<PatchStudentEnrollmentCommand>
{
    public PatchStudentEnrollmentCommandValidator()
    {
        RuleFor(x => x.Request.Id).GreaterThan(0);
        RuleFor(x => x.Request.AcademicYearId).GreaterThan(0).When(x => x.Request.AcademicYearId.HasValue);
        RuleFor(x => x.Request.ClassId).GreaterThan(0).When(x => x.Request.ClassId.HasValue);
        RuleFor(x => x.Request.SectionId).GreaterThan(0).When(x => x.Request.SectionId.HasValue);
    }
}
