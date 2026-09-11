using FluentValidation;

namespace SchoolERP.Application.Features.Student.Commands.StudentEnrollment.BulkUpdateStudentEnrollment;

public class BulkUpdateStudentEnrollmentCommandValidator : AbstractValidator<BulkUpdateStudentEnrollmentCommand>
{
    public BulkUpdateStudentEnrollmentCommandValidator()
    {
        RuleFor(x => x.Request.Ids).NotEmpty();
        RuleFor(x => x.Request.AcademicYearId).GreaterThan(0);
        RuleFor(x => x.Request.ClassId).GreaterThan(0);
        RuleFor(x => x.Request.SectionId).GreaterThan(0);
    }
}
