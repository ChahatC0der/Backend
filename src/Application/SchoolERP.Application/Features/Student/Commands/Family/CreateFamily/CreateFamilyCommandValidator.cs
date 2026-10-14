using FluentValidation;

namespace SchoolERP.Application.Features.Student.Commands.Family.CreateFamily;

public class CreateFamilyCommandValidator : AbstractValidator<CreateFamilyCommand>
{
    public CreateFamilyCommandValidator()
    {
        RuleFor(x => x.Request.HeadName).MaximumLength(255).When(x => !string.IsNullOrEmpty(x.Request.HeadName));
        RuleFor(x => x.Request.HeadMobile).MaximumLength(20).When(x => !string.IsNullOrEmpty(x.Request.HeadMobile));
        RuleFor(x => x.Request.HeadEmail).EmailAddress().When(x => !string.IsNullOrEmpty(x.Request.HeadEmail));
    }
}
