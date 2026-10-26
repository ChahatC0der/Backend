using FluentValidation;

namespace SchoolERP.Application.Features.Fees.Commands.Payment.RecordAdvancePayment;

public sealed class RecordAdvancePaymentCommandValidator : AbstractValidator<RecordAdvancePaymentCommand>
{
    public RecordAdvancePaymentCommandValidator()
    {
        RuleFor(x => x.Request.StudentId).GreaterThan(0);
        RuleFor(x => x.Request.Amount).GreaterThan(0);
        RuleFor(x => x.Request.Mode)
            .Must(m => m is "cash" or "cheque" or "bank_transfer" or "online" or "card")
            .WithMessage("Mode must be one of: cash, cheque, bank_transfer, online, card.");
    }
}
