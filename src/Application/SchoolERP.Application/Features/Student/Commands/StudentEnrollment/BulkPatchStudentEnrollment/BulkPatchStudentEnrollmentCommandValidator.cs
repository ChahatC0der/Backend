using FluentValidation;

namespace SchoolERP.Application.Features.Student.Commands.StudentEnrollment.BulkPatchStudentEnrollment;

public class BulkPatchStudentEnrollmentCommandValidator : AbstractValidator<BulkPatchStudentEnrollmentCommand>
{
    public BulkPatchStudentEnrollmentCommandValidator()
    {
        RuleFor(x => x.Request.Ids).NotEmpty();
    }
}
