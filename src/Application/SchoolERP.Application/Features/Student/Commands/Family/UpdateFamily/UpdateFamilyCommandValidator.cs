using FluentValidation;

namespace SchoolERP.Application.Features.Student.Commands.Family.UpdateFamily;

public class UpdateFamilyCommandValidator : AbstractValidator<UpdateFamilyCommand>
{
    public UpdateFamilyCommandValidator()
    {
        RuleFor(x => x.Request.Id).GreaterThan(0);
        RuleFor(x => x.Request.HeadEmail).EmailAddress().When(x => !string.IsNullOrEmpty(x.Request.HeadEmail));
    }
}
