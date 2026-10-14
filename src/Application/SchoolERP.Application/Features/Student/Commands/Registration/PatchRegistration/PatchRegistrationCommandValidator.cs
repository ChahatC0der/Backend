using FluentValidation;

namespace SchoolERP.Application.Features.Student.Commands.Registration.PatchRegistration;

public class PatchRegistrationCommandValidator : AbstractValidator<PatchRegistrationCommand>
{
    public PatchRegistrationCommandValidator()
    {
        RuleFor(x => x.Request.Id).GreaterThan(0);
        RuleFor(x => x.Request.StudentFirstName).MaximumLength(255).When(x => !string.IsNullOrEmpty(x.Request.StudentFirstName));
        RuleFor(x => x.Request.ParentPrimaryMobile).MaximumLength(20).When(x => !string.IsNullOrEmpty(x.Request.ParentPrimaryMobile));
    }
}
