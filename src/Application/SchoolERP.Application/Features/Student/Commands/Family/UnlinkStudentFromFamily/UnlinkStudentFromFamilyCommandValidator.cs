using FluentValidation;

namespace SchoolERP.Application.Features.Student.Commands.Family.UnlinkStudentFromFamily;

public class UnlinkStudentFromFamilyCommandValidator : AbstractValidator<UnlinkStudentFromFamilyCommand>
{
    public UnlinkStudentFromFamilyCommandValidator()
    {
        RuleFor(x => x.Request.FamilyId).GreaterThan(0);
        RuleFor(x => x.Request.StudentId).GreaterThan(0);
    }
}
