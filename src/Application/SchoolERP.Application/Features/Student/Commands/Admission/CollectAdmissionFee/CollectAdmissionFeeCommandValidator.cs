using FluentValidation;

namespace SchoolERP.Application.Features.Student.Commands.Admission.CollectAdmissionFee;

public class CollectAdmissionFeeCommandValidator : AbstractValidator<CollectAdmissionFeeCommand>
{
    public CollectAdmissionFeeCommandValidator()
    {
        RuleFor(x => x.Request.Id).GreaterThan(0);
        RuleFor(x => x.Request.Amount).GreaterThan(0);
        RuleFor(x => x.Request.PaymentMode).NotEmpty()
            .Must(m => new[] { "online", "offline", "waived" }.Contains(m));
    }
}
