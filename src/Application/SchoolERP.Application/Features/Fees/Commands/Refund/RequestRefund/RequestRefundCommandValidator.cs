using FluentValidation;

namespace SchoolERP.Application.Features.Fees.Commands.Refund.RequestRefund;

public sealed class RequestRefundCommandValidator : AbstractValidator<RequestRefundCommand>
{
    public RequestRefundCommandValidator()
    {
        RuleFor(x => x.Request.StudentId).GreaterThan(0);
        RuleFor(x => x.Request.Amount).GreaterThan(0);
        RuleFor(x => x.Request.Reason)
            .Must(r => r is "withdrawal" or "overpayment" or "cancellation" or "other")
            .WithMessage("Reason must be one of: withdrawal, overpayment, cancellation, other.");
        RuleFor(x => x.Request.PaymentMode)
            .Must(m => m is "cash" or "cheque" or "bank_transfer" or "online" or "card")
            .WithMessage("PaymentMode must be one of: cash, cheque, bank_transfer, online, card.");
    }
}
