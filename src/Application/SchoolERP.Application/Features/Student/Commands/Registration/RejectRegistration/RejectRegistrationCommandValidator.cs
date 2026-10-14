using FluentValidation;

namespace SchoolERP.Application.Features.Student.Commands.Registration.RejectRegistration;

public class RejectRegistrationCommandValidator : AbstractValidator<RejectRegistrationCommand>
{
    public RejectRegistrationCommandValidator()
    {
        RuleFor(x => x.Request.Id).GreaterThan(0);
        RuleFor(x => x.Request.RejectionReason).NotEmpty().MaximumLength(1000);
    }
}
