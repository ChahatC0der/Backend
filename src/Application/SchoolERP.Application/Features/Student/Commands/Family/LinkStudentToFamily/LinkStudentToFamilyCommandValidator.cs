using FluentValidation;

namespace SchoolERP.Application.Features.Student.Commands.Family.LinkStudentToFamily;

public class LinkStudentToFamilyCommandValidator : AbstractValidator<LinkStudentToFamilyCommand>
{
    public LinkStudentToFamilyCommandValidator()
    {
        RuleFor(x => x.Request.FamilyId).GreaterThan(0);
        RuleFor(x => x.Request.StudentId).GreaterThan(0);
    }
}
