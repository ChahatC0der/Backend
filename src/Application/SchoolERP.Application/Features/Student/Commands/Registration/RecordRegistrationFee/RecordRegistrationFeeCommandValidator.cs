using FluentValidation;

namespace SchoolERP.Application.Features.Student.Commands.Registration.RecordRegistrationFee;

public class RecordRegistrationFeeCommandValidator : AbstractValidator<RecordRegistrationFeeCommand>
{
    public RecordRegistrationFeeCommandValidator()
    {
        RuleFor(x => x.Request.Id).GreaterThan(0);
        RuleFor(x => x.Request.Amount).GreaterThan(0);
        RuleFor(x => x.Request.PaymentMode).NotEmpty()
            .Must(m => new[] { "online", "offline", "waived" }.Contains(m));
    }
}
