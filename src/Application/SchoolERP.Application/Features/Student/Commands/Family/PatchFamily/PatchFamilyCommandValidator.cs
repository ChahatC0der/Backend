using FluentValidation;

namespace SchoolERP.Application.Features.Student.Commands.Family.PatchFamily;

public class PatchFamilyCommandValidator : AbstractValidator<PatchFamilyCommand>
{
    public PatchFamilyCommandValidator()
    {
        RuleFor(x => x.Request.Id).GreaterThan(0);
        RuleFor(x => x.Request.HeadEmail).EmailAddress().When(x => !string.IsNullOrEmpty(x.Request.HeadEmail));
    }
}
