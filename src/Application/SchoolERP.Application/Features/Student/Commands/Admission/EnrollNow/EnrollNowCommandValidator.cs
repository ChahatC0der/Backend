using FluentValidation;

namespace SchoolERP.Application.Features.Student.Commands.Admission.EnrollNow;

public class EnrollNowCommandValidator : AbstractValidator<EnrollNowCommand>
{
    public EnrollNowCommandValidator()
    {
        RuleFor(x => x.Request.Id).GreaterThan(0);
    }
}
